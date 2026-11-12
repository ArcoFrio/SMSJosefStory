# Notes for agents working here

## Game installs (moved 2026-11-12)

The author moved Starmaker Story to a new place, under an
opaque folder name. Where it is now is in a single local file outside every repository, shared by all the SMS*
repos and never committed: `%USERPROFILE%\.sms\game.props` (`StarmakerGame` is the author's game; the
`Starmaker*Test` properties are the other repos' test copies).

- `SMSJosefStory.csproj` imports it: a Debug build goes into `$(StarmakerGame)\BepInEx\plugins\`, or into
  `bin\Debug\` on a machine without that file. If the game moves, change that file only.
- **Never write that path into a tracked file**, a commit message or anything pushed: name the property instead.
  Don't rename the folder.
