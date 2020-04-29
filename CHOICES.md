# Choices of technologies
In this document we will briefly outline the reasoning for choosing the
technologies that we chose, Later we will explain any positive or negative
effects that are results of these choices.

## Programming Language: C#
Early on we had to convert the existing system into a new programming language. 
We had a short discussion about programming languages, and it was argued that C#
was the language that most people would be able to write from the get go. We
wanted to focus less on the development of the system, and rather on setting up
DevOps tools and things in that regard, so picking a challenging language was
not prioritized, rather something that would make the code itself somewhat easy
to debug, seemed more sensible.

### Evaluation
The team probably has varying opinions on the C# language, and some of use would
probably have preferred somewhat that was more engaging or faster to write,
however it got the job done, and it did make it easier to debug, leaving time
for writing various tests.

## Database: MSSQL
We did not want to use SQLite in production, so we had to change to another
database scheme. Due to already picking C# for the programming language, it
seemed to make sense to continue the development in the same technology sphere,
by picking yet another microsoft product. This also provided the best possible
object relation mapping (ORM), which made development somewhat easier.


### Evaluation
The only problem we've had with this, was that a subset of our development team
continued to use SQLite for local testing, and SQLite has a more relaxed
relation to constraint, so some errors would occure in production that didn't
locally. However we fixed this by making it easier to spin up a MSSQL database
locally. 
However MSSQL in itself provided no problems - it had exactly the features that
we were after, and worked like a charm. This seemed to have been a good choice.

## Testing strategy - unit-tests
To help increase our confidence in the changes added to the system we added
unit tests. The unit-tests are written in C# using the XUnit test framework,
which is used to test functionality as implemented in web application
(e.g., creating a user, adding a message, etc.).

The unit tests are focused around the service classes, which implements the
business logic related to the functionality of the system. These tests aims
to check the "happy-path" where the function succeeds as well as the expec-
ted error paths (i.e., adding a message to an unknown user). The tests are
executed using the built-in tooling of the dotnet CLI included in the .NET
Core SDK.

These tests are run as part of the CI pipeline on pull requests
(PR), and when a PR is merged into the master branch.

### Evaluation
The unit-tests are quick to run in the CI pipeline. However, the amount of
code currently covered by the tests are limited to the primary functiona-
lity. This is, however, not a reflection of the choice of strategy, but rather the available time we allocated to this part of the project. 

Some tests are without a doubt better than no tests, and we did catch some bad exceptions once in a while. Using the "happy-path" mentality strategy did catch the worst of errors however, meaning that it definitely added value to the project as a whole, however there is naturally always room for improvement. The focus did, however, mean that we tested wide, and meant that most features were covered.
The unit-tests are quick to run in the CI pipeline. However, the amount of
code currently covered by the tests are limited to the primary functiona-
lity. 

## Virtualization / Containerization: Docker & Docker-compose
There are a few different alternatives to docker, but it is essentially the
de-facto standard in the business. It was also what was introduced in the
course, and seemed rather interesting. The alternative would have been vagrant,
but as [argued
here](https://stackoverflow.com/questions/16647069/should-i-use-vagrant-or-docker-for-creating-an-isolated-environment),
it is probably less ideal. 

We wanted to get more experience with docker. The syntax and tool can be
extended to be used in a wide variety of similar tools, like Kubernetes or 

### Evaluation
We would probably prefer having a more powerful host for the containers in the
future. If we had to scale vertically it would presumably be difficult, and this
is handled better in systems like Kubernetes, to the best of our knowledge.
There are a large variety of different tools, which still builds on the docker
syntax, that has a bigger set of features, that would presumably handle scaling
better, however it worked for the relatively small service that we had to
provide - so for the setting, it was probably an ideal choice, however for
larger systems we would probably pick something else.

If we hadn't want to focus on something that would provide learning, we would
probably have picked Azure and focus fully on the Microsoft stack, as we
essentially started working with that stack, and Azure does provide a lot of
easy to use tools for a variety of needs. Whether using Azure is a ideal,
however, is a matter of discussion in the group, and is based on both political
and personal bias and opinions.

## OS: Ubuntu 18.04.3 LTS 
We wanted an OS that had long term support (LTS) as to make sure we had the
least amount of bugs or security holes, and if any surfaced, then those would be
fixed accordingly.

We wanted to use a Linux based system as that seemed to give the greatest level
of learning. Creating a windows WM would give us a graphical user interface and
that would definitely yield some learning as well, but seemed less relevant in
the context of this course. We could have used an Arch Linux or other
distribution, however Ubuntu is rather common and has a great community making
it somewhat easy to figure out how things are done. This made it ideal for the
members of the group that didn't have a great level of proficiency. 

The OS was also less relevant regarding the software we would be running

### Evaluation
As we are focusing on automating all the boring steps, we actually didn't have
to do a lot of things at the OS level, so the choice was rather irrelevant after
having initially set everything up, but it was nice that we wouldn't need a
singular sysadmin, but rather used a tool everyone would be able to use to some
degree, though with varying level of efficiency. 

The tool provided learning both for the Linux proficient, as they learned things
about docker, but it also provided a safe learning environment for the Windows
users.

## Host: Digital Ocean
We had to host our solution somewhere to have a production environment. The
courses presented Digital Ocean, and a couple of us already had experience with
it. However the main factor was probably the Github Education Pack that provides
a bunch of free credit, meaning we wouldn't have to pay for any hosting for the
duration of the course. 

### Evaluation
We initially provisioned a relatively small droplet, which is what they call
their virtual private servers (VPS), however as we needed to add more monitoring
and logging we had a growing requirement for the specs of the VPS. We ended up
having to provision multiple droplets, because scaling vertically would give us
some downtime, which we wanted to avoid. We ended up having a VPS for our tools
and one for our solution. This was probably a good thing, however, as that makes
sure that even if one of the two crashes the other wont necessarily. However, we
would still have issues if we wanted to scale the solution itself, which would
essentially be impossible without some downtime - as our service isn't completely
stateless, we wouldn't just be able to scale vertically neither. 

In any professional context it might have been a better choice to use some
containerization-as-a-service solution or something where the features were
managed, however that naturally depends on the project at hand, however this
left us with a lot of manually work, setting up docker etc and scaling issues.
However, for the context of this course it was a great pick, as we learned a lot
from having to work with the OS ourselves, rather than, for instance, hosting
everything on Heroku, which provides hosting for a docker-container and
automatically handles everything without any critical downtime.

## Monitoring: Prometheus & Grafana
We chose Prometheus as the primary monitoring tool and Grafana for data visualization.
No one from the team had a lot of experience working with monitoring tools. It meant that no one had expectations or any preference regarding choosing the monitoring tool. 
Taking a look at [Prometheus's comparison to alternatives](https://prometheus.io/docs/introduction/comparison/) (even though it should be taken with a grain of salt as they
made the comparison themself) it made it clear that it did fit into the setup because Prometheus is designed to
monitor targets as in servers, containers and the like.

Also, supporting active monitoring by periodically scrap our application by pulling data from this target.
A pull-based system enables us to rate control in which it will pull the data. 
With a push-based system we may have the risk of sending too much data towards our server and in worst case crash it. 

To set up Prometheus and Grafana is just a matter of creating docker containers. 

### Evaluation

In our case, it must integrate well into our current setup, and it is well-supported. That is, there is some kind of official
library for the tool that we want to use, and it is actively maintained.
[Prometheus has an official GitHub repository for .NET](https://github.com/prometheus-net/prometheus-net) with examples of getting started which fits with our criteria.
Alternatives like Graphite there is no official GitHub repository, and a [simple search on Github](https://github.com/search?q=graphite+.net) reveals it. It also the fact that if the community is large enough
there is a possibility of finding a solution to your problem in a short amount of time.


## Logging: ELK

We chose Elasticsearch, Logstash and Kibana (ELK stack) as logging tools.
Firstly, it enables us to do modern, scalable and user-friendly logging, and it is also one of the most popular
choices at the moment.
Because it promotes centralized logging we chose to set up the ELK stack on another droplet, and the fact that
Elasticsearch requires a larger amount of memory.

To setup the ELK stack is just a matter of creating docker containers. 

### Evaluation
As we prioritize ease of integration of the toolset that we choose, it felt that the ELK stack was the right choice.
We tried to look for alternatives to avoid making the easy choice of choosing the most popular one.
We had a look at [LogDNA](https://logdna.com) that is Elasticsearch and Kibana combined. It also supports containerized 
environments even though they suggest using Kubernetes with their product.
We want to ensure that libraries used for this product are well-supported and maintained for .NET. We found a Git
Repository named [RedBear.LogDNA](https://github.com/RedBearSys/RedBear.LogDNA) where a library resides for connecting to logDNA from .NET. 
Studying the issues that were made did make the choice easy for us as some issues stated that the library leads to system
failure was not something that we wanted to work with. 
Furthermore, it should be simple to acquire help which did not look like to be the case.

## Integration tests: Python3.8
We wanted to test the API used by the simulator to make sure that this would
continue to work - Also as this surface is a API it is easier to test than a
graphical user interface, so we would be able to test all backend functionality
relatively easy.

The simulator that had to interface with our service was already written in
python3, so testing if the simulator would be able to work, would, in theory, be
as simple as running the simulator on a clean database, and see if the simulator
failed. It would still have to be rewritten a bit though


As a member of our team has worked with python professionally for multiple years
he proposed he would convert it into unit tests. This made it somewhat easy to
isolate where errors occurred if something failed within this tester.

Python being fast to prototype in and great for scripts, also made this an ideal
choice for blackbox testing. 

### Evaluation
This has so far proven to be a great choice. We haven't had to modify the files
yet, so the primary factor seemed to be development time, which was low, so the
choice seemed perfect.

## Continuous Integration: Github Actions
We initially looked at a variety of different CI possibilities, and considered
Jenkins for one, however we randomly looked at github and a group member
asked if any of us have every tried using Github Actions, their CI solution, and
we realized that neither of us had. This seemed like a great way to try
something new and it also seemed ideal with the current stack we were running. 
The choice was mainly based on this: the availability and the possibility to
learn something new. 

### Evaluation
It was really neat having it integrate so smoothly with our pull request flow

## Packaging Host: Github
Github has the possibility to host and provide packages for downloading, much
like Dockerhub. We considered moving our images to Dockerhub, but having in
centralized one place seemed ideal. We liked the idea of having a few select
services that we relied on, as to not create a too complex development flow, but
rather utilize few tools that integrated nicely together and provided each a
large chunk of the features required.

### Evaluation
We had a few issues, especially that you [cannot download without being logged
in](https://github.community/t5/GitHub-API-Development-and/Download-from-Github-Package-Registry-without-authentication/td-p/35255),
which was rather tedious, but we overcame it by having one of our personal
tokens stored on the production site (which is less than ideal, but works).
Other than that it provided the features we needed. Looking at the features we
use, we would have gotten nothing more out of using Dockerhub, so having it all
centralized one place, packaging, repository & CI seemed like a neat choice, on
that we stand by.



## Continuous Delivery: Watchtower
We needed our production server to automatically update whenever the master
branch updated on our repository - which was when ever a new version was
released on the packaging provider. There is a few different tools for this, and
this is something Jenkins does provide, however installing Jenkins for this
feature alone, seemed overkill and would use a magnitude more CPU power than we
wanted. So we found a service called Watchtower, which runs as a docker
container, and with with a consistant interval checks whether the remote image
has been updated - if it has, it updates the running container. 

### Evaluation
It worked as expected and we didn't really touch it after installing it. This was
lightweight and provided exactly what we needed. Some critique can be made of
the service, as it in theory has access to the whole docker system, which might
be dangerous, and we are not exactly sure how it handles any errors in building
a new package, so it is probably more error prone, however it works for our
limited need. If we were to scale up and use this in a more system critical
setting, we would probably research this more, and find a enterprise grade
solution. Maybe even giving the github actions direct access to the Docker
service, and giving it the possibility to update the service directly from a
Github Action. We didn't do this initially out of fear for opening up our docker
host to the world, as that seemed relatively risky. However, with more research,
we could probably find a way to do this in a secure way.

## Issue tracking / Kanban: Github 
Generally speaking we never really put much thought into how we would track
issues and how we would separate the tasks at hand. As we already had Github
open, we simply created all our tasks on the issue board there and never thought
about alternatives. 
Alternatively we could have created a Trello board or a Jira project, however
with the limited scope of the project it seemed extensive to include a whole
other system just for task management. As previously mentioned we generally
tried limiting the number of different tools we used, and create a stack with as
few different tools as possible.

### Evaluation
We definitely had problems with our taskmanagement and ended up doing some of the
tasks too late, so we definitely had to change our workflow, and would have if
we could do it over. I think the main issue was that we didn't consult the issue
list often enough, and possibly didn't put deadlines on, as well as not
assigning people to issues. Ideally we should probably have improved our overall
development process earlier on, but this is covered in the [Post
Mortem](postmortem.md). We probably wouldn't have gotten any alternative
important features by choosing another service, as the problems we had were
based on structural team problems rather than the tool itself. Having the issues
closely aligned with the pull-request flow was definitely a helpful feature.
