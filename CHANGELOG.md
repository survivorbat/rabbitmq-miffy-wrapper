# Changelog Miffy Framework

## 0.9.0

- Rename several senders/receivers to make naming consistent
- Add comments notifying the user that a component is low-level
- Add a Publish() method to the command publisher
- Allow a custom command return type to be used in command listeners

## 0.8.4-0.8.5

- Add more tests to bring code coverage above 98%

## 0.8.3

- Add several tests

## 0.8.2

- Bring back single pipeline

## 0.8.1

- Ensure that loggers can only be set once

## 0.8.0

- Add better listener method evaluation
- Separate pipeline into 3 pieces
- Allow strings to be input types for event listeners
- Migrate changelog to individual packages ;-)

## 0.7.1

- Removed test dependency of microservices project.

## 0.7.0

- Command and Event listener attributes now belong on top of the method instead of the class
- Add a CHANGELOG.md file

## 0.6.4

- Make sure queues are not automatically removed and add test cleanup.

## 0.6.3

- Add exception message to critical log statement.
- Add README information concerning serializable exceptions.

## 0.6.2

- Added critical log statement to log unresolved dependencies.

## 0.6.0-0.6.1

- Replace CommandError's ExceptionMessage with a full-fledged error.

## <0.6.0

Changes were not properly recorded before this version.
