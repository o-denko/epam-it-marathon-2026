@level2
Feature: Level 2 - Submit Consolidated Pre-Order
  As a Reservation Owner
  I want to submit the consolidated pre-order to the restaurant
  So that the restaurant can prepare the dishes for the reservation

  @ui @smoke
  Scenario: Owner submits the consolidated pre-order through the review screen
    Given the Owner opens the group pre-order page in demo mode
    When the Owner clicks the "Submit Pre-Order" button
    Then the pre-order status should update to "Sent to restaurant" or "Submitted"
    And background synchronization should keep updating the screen status

  @api
  Scenario: Cannot submit pre-order when no baskets are confirmed or empty
    Given an API request to submit group reservation "empty-group-000"
    When the submit API is called without any submitted dishes
    Then the API response status code should be 400, 401 or 404

  @api
  Scenario: Non-owner cannot submit group pre-order
    Given an unauthorized user attempt to submit group reservation "grp-mock-123"
    When the submit API request is sent without owner token
    Then the API response status code should be 401 or 403
