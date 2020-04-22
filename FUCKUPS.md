# Fuck ups

We did a list of things that had horrible effect, however we learned from them,
and now know what we should have done instead. This file lists the various
things we did wrong, why, and what we would have done differently in the future.

## Not seeing production exceptions
For the first month, we didn't implement any sort of great logging that would
highlight all the uncaught exceptions we had in production. This meant that we
had a couple of days with downtime once in a while, without realizing it till it
was too late. This is obviously really bad. we ended up utilizing sentry.io to
solve this problem, however we had already lost a ton of users due to the lack
of possibility to sign up.

### What we would have done differently
Implemented sentry.io earlier would have fixed it.

## Losing the database in system update
TL;DR (please write more someone) we didn't mount or database in a volume

## Losing the database volume when migrating to Docker Swarm
When we went from normal docker to docker swarm mode, we somehow didn't use the
same database volume. This is probably because the naming of volumes are
prefixed with the context of which it is created in `docker-compose`, which we
utilize.
This meant that docker created a new volume, and we didn't really check this.
Without active monitoring of our logging software we weren't fully aware that
users were dropped and therefore didn't see the error. Also we didn't go through
the system thoroughly after the migration, so we didn't realize that something
was wrong until a few days later. We then had new users in the new database and
old in the old.

### What we would have done differently
A couple of things would help. Naturally we should have created a backup,
even though this wasn't an issue it would have been a good idea. Additionally we
should have done some quick tests to validate that the production database
wasn't empty (by checking whether the feed was empty on the website). This would
have done a lot and made it easy to fix and mount the correct volume. An other
thing that would have helped was setting up chatops. Having the error log not
sent to a specific developer by email but rather in a chat we all had access
too, would have helped. Additionally we could have monitored the monitoring and
logs as well easily and created various triggers (the amount of 4XX errors
presumably increased afterwards, which would have been nice to know, even those
these are not normally errors we are concerned with as they are not uncaught exceptions).


