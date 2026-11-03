@level1
Feature: Level 1 - View Group Pre-Order Participants
  As a Reservation Owner
  I want to view the list of participants, their dishes, and totals
  So that I can monitor the pre-order status of the entire group

  @ui @smoke
  Scenario: Owner reviews all joined participants and totals in the UI
    Given the Owner opens the group pre-order page in demo mode
    When the page loads successfully
    Then the Owner should see all joined participant cards
    And the Grand Total should be calculated from all submitted dishes

  @ui
  Scenario: Warning banner is displayed when some participants are still choosing
    Given the Owner opens the group pre-order page in demo mode
    When there are participants with unsubmitted baskets
    Then a warning banner informing about unsubmitted participants should be displayed

  @api
  Scenario: Non-owner client cannot view group sessions
    Given an unauthorized or non-owner user request to view sessions for group "grp-mock-123"
    When the API request is sent without valid owner credentials
    Then the API response status code should be 401 or 403

  @api
  Scenario: Requesting sessions for a non-existing group returns 404 or 401
    Given a request to view sessions for non-existing group "non-existent-group-999"
    When the API request is sent
    Then the API response status code should be 401, 403 or 404
