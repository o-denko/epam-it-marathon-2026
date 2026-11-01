import { CurrencyPipe, DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import {
  catchError,
  EMPTY,
  exhaustMap,
  finalize,
  forkJoin,
  interval,
  of,
  Subscription,
   switchMap,
  tap,
} from 'rxjs';
import { ParticipantCardComponent } from '@app/components/group-orders/participant-card/participant-card.component';
import { GroupReservationStatus } from '@app/models/group-orders/group-status.model';
import { Participant } from '@app/models/group-orders/participant.model';
import { GuestInvite } from '@app/models/reservations/guest-invite.model';
import { GroupOrdersService } from '@app/services/group-orders/group-orders.service';
import { ToastService } from '@app/services/toast/toast.service';
import { ModalComponent } from '@app/shared/custom-components/modal/modal.component';
import { consolidatePreOrder } from '@app/shared/helpers/consolidate-pre-order';

const SYNC_INTERVAL_MS = 15_000;

@Component({
  selector: 'app-group-order-page',
  imports: [CurrencyPipe, DatePipe, ParticipantCardComponent, ModalComponent],
  templateUrl: './group-order-page.component.html',
  styleUrl: './group-order-page.component.scss',
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class GroupOrderPageComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly groupOrdersService = inject(GroupOrdersService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  private syncSub?: Subscription;
  private isSyncInFlight = false;

  readonly reservationId = this.route.snapshot.paramMap.get('reservationId') ?? '';

  readonly invite = signal<GuestInvite | null>(null);
  readonly participants = signal<Participant[]>([]);
  readonly ownerSessionId = signal('');
  readonly status = signal<GroupReservationStatus>('Open');

  readonly isLoading = signal(true);
  readonly errorState = signal<'not-found' | 'error' | null>(null);

  readonly lastUpdatedAt = signal<Date | null>(null);
  readonly syncError = signal(false);

  readonly showSubmitModal = signal(false);
  readonly isSubmitting = signal(false);

  readonly participantToRemove = signal<Participant | null>(null);
  readonly isRemoving = signal(false);

  readonly consolidated = computed(() => consolidatePreOrder(this.participants()));

  readonly hasAnyDishes = computed(() =>
    this.participants().some((person) => person.items.length > 0),
  );

  readonly unsubmittedCount = computed(
    () => this.participants().filter((person) => !person.isSubmitted).length,
  );

  readonly unsubmittedWarning = computed(() => {
    const count = this.unsubmittedCount();
    if (count === 0) {
      return '';
    }
    return count === 1
      ? "1 participant hasn't submitted their cart yet — their dishes won't be included in the order"
      : `${count} participants haven't submitted their cart yet — their dishes won't be included in the order`;
  });

  readonly statusBadgeText = computed(() =>
    this.status() === 'Submitted' ? 'Sent to Restaurant' : 'Collecting pre-orders',
  );

  readonly canSubmit = computed(
    () =>
      this.status() === 'Open' &&
      this.consolidated().items.length > 0 &&
      !this.isSubmitting(),
  );

  constructor() {
    this.destroyRef.onDestroy(() => this.stopSync());
  }

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.stopSync();
    this.isLoading.set(true);
    this.errorState.set(null);

    this.groupOrdersService
      .getGroupInvite$(this.reservationId)
      .pipe(
        switchMap((invite) =>
          forkJoin({
            invite: of(invite),
            sessions: this.groupOrdersService.getSessions$(invite.GroupReservationId),
            ownerSessionId: this.groupOrdersService.getOwnerSessionId$(this.reservationId),
            status: this.groupOrdersService.getStatus$(invite.GroupReservationId),
          }),
        ),
      )
      .subscribe({
        next: ({ invite, sessions, ownerSessionId, status }) => {
          this.invite.set(invite);
          this.participants.set(sessions.participants);
          this.ownerSessionId.set(ownerSessionId);
          this.status.set(status.status);
          this.lastUpdatedAt.set(new Date());
          this.syncError.set(false);
          this.isLoading.set(false);

          if (status.status === 'Open') {
            this.startSync();
          }
        },
        error: (err: unknown) => {
          this.isLoading.set(false);
          if (err instanceof HttpErrorResponse && err.status === 404) {
            this.errorState.set('not-found');
          } else {
            this.errorState.set('error');
          }
        },
      });
  }

  refreshNow(): void {
    this.performSilentSync$().subscribe();
  }

  openSubmitModal(): void {
    if (!this.canSubmit()) {
      return;
    }
    this.showSubmitModal.set(true);
  }

  closeSubmitModal(): void {
    if (this.isSubmitting()) {
      return;
    }
    this.showSubmitModal.set(false);
  }

  confirmSubmit(): void {
    const currentInvite = this.invite();
    if (this.isSubmitting() || !currentInvite) {
      return;
    }

    this.isSubmitting.set(true);
    this.groupOrdersService
      .submit$(currentInvite.GroupReservationId, this.reservationId)
      .subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.showSubmitModal.set(false);
          this.status.set('Submitted');
          this.stopSync();
          this.toastService.success('Pre-order sent to the restaurant');
        },
        error: () => {
          this.isSubmitting.set(false);
          this.showSubmitModal.set(false);
        },
      });
  }

  openRemoveModal(participant: Participant): void {
    if (this.status() !== 'Open' || participant.type !== 'Guest') {
      return;
    }
    this.participantToRemove.set(participant);
  }

  closeRemoveModal(): void {
    if (this.isRemoving()) {
      return;
    }
    this.participantToRemove.set(null);
  }

  confirmRemoveGuest(): void {
    const guest = this.participantToRemove();
    const currentInvite = this.invite();
    if (!guest || !currentInvite || this.isRemoving()) {
      return;
    }

    this.isRemoving.set(true);
    this.groupOrdersService
      .removeSession$(currentInvite.GroupReservationId, guest.id)
      .subscribe({
        next: (updatedSessions) => {
          this.participants.set(updatedSessions.participants);
          this.isRemoving.set(false);
          this.participantToRemove.set(null);
        },
        error: () => {
          this.isRemoving.set(false);
          this.participantToRemove.set(null);
        },
      });
  }

  private startSync(): void {
    this.stopSync();
    this.syncSub = interval(SYNC_INTERVAL_MS)
      .pipe(exhaustMap(() => this.performSilentSync$()))
      .subscribe();
  }

  private stopSync(): void {
    this.syncSub?.unsubscribe();
    this.syncSub = undefined;
  }

  private performSilentSync$() {
    const groupId = this.invite()?.GroupReservationId;
    if (!groupId || this.status() !== 'Open' || this.isSyncInFlight) {
      return EMPTY;
    }

    this.isSyncInFlight = true;
    return forkJoin({
      sessions: this.groupOrdersService.getSessions$(groupId, { silent: true }),
      status: this.groupOrdersService.getStatus$(groupId, { silent: true }),
    }).pipe(
      tap(({ sessions, status }) => {
        this.participants.set(sessions.participants);
        this.status.set(status.status);
        this.lastUpdatedAt.set(new Date());
        this.syncError.set(false);
        if (status.status !== 'Open') {
          this.stopSync();
        }
      }),
      catchError(() => {
        this.syncError.set(true);
        return EMPTY;
      }),
      finalize(() => {
        this.isSyncInFlight = false;
      }),
    );
  }
}
