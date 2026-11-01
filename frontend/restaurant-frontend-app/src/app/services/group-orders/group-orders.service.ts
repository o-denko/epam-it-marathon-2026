import { HttpClient, HttpContext } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable, switchMap } from 'rxjs';
import { SKIP_ERROR_TOAST } from '@app/auth/skip-error-toast.context';
import { GroupSessionsResponse } from '@app/models/group-orders/group-sessions-response.model';
import { GroupStatusResponse } from '@app/models/group-orders/group-status.model';
import { OwnerSessionResponse } from '@app/models/group-orders/owner-session.model';
import {
  SubmitGroupPreOrderRequest,
  SubmittedDish,
} from '@app/models/group-orders/submit-group-pre-order.model';
import { GroupInviteToken, GuestInvite } from '@app/models/reservations/guest-invite.model';
import { API_BASE_URL } from '@app/shared/config/api-base-url.token';

export interface GroupOrderRequestOptions {
  silent?: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class GroupOrdersService {
  private readonly baseUrl = inject(API_BASE_URL);
  private readonly http = inject(HttpClient);

  getGroupInvite$(reservationId: string): Observable<GuestInvite> {
    return this.http
      .get<GroupInviteToken>(
        `${this.baseUrl}/reservations/${encodeURIComponent(reservationId)}/invite-token`,
      )
      .pipe(
        switchMap(({ token }) =>
          this.http.get<GuestInvite>(`${this.baseUrl}/guest/invites/${encodeURIComponent(token)}`),
        ),
      );
  }

  getSessions$(
    groupReservationId: string,
    options?: GroupOrderRequestOptions,
  ): Observable<GroupSessionsResponse> {
    const context = new HttpContext().set(SKIP_ERROR_TOAST, options?.silent === true);
    return this.http.get<GroupSessionsResponse>(
      `${this.baseUrl}/guest/groups/${encodeURIComponent(groupReservationId)}/sessions`,
      { context },
    );
  }

  getOwnerSessionId$(reservationId: string): Observable<string> {
    return this.http
      .get<OwnerSessionResponse>(
        `${this.baseUrl}/reservations/${encodeURIComponent(reservationId)}/guest-session-id`,
      )
      .pipe(map((res) => res.guestSessionId));
  }

  getStatus$(
    groupReservationId: string,
    options?: GroupOrderRequestOptions,
  ): Observable<GroupStatusResponse> {
    const context = new HttpContext().set(SKIP_ERROR_TOAST, options?.silent === true);
    return this.http.get<GroupStatusResponse>(
      `${this.baseUrl}/guest/groups/${encodeURIComponent(groupReservationId)}/status`,
      { context },
    );
  }

  submit$(groupReservationId: string, reservationId: string): Observable<SubmittedDish[]> {
    const body: SubmitGroupPreOrderRequest = { reservationId };
    return this.http.post<SubmittedDish[]>(
      `${this.baseUrl}/group-reservations/${encodeURIComponent(groupReservationId)}/submit`,
      body,
    );
  }

  removeSession$(
    groupReservationId: string,
    guestSessionId: string,
  ): Observable<GroupSessionsResponse> {
    return this.http.delete<GroupSessionsResponse>(
      `${this.baseUrl}/guest/groups/${encodeURIComponent(groupReservationId)}/sessions/${encodeURIComponent(guestSessionId)}`,
    );
  }
}
