@level3
Feature: Level 3 - Remove Participant from Group Pre-Order
  As a Reservation Owner
  I want to remove a participant from the group pre-order
  So that their dishes are not included in the final restaurant order

  @ui @bonus
  Scenario: Owner removes a guest participant from the UI screen
    Given the Owner opens the group pre-order page in demo mode
    When the Owner removes a guest participant
    Then the participant card should disappear from the list
    And the Grand Total should be recalculated without the removed guest

  @api
  Scenario: Owner cannot delete their own session without token
    Given an Owner session ID in group "grp-mock-123"
    When the Owner tries to delete their own session via API
    Then the API response status code should be 400 or 401

  @api
  Scenario: Removing non-existent participant returns 404 or 401
    Given a non-existent participant "non-existent-session-999" in group "grp-mock-123"
    When the delete participant API request is sent
    Then the API response status code should be 401, 403 or 404
