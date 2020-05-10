# Development Report

This document is the final report of Group b (2k20 MSc) for the _DevOps,
Software Evolution and Software Maintenance_ course at IT University of
Copenhagen, held in spring 2020. It will detail the process of migrating the
Minitwit platform (a really simple social network similar to Twitter),
refactoring of its system, major devops tasks we have done throughout the
course, our team and repository organization and strategies. Report is divided
by sections, not by the timeline, so the reader can get better overview of our
work. We also argued for the choice of technologies and decisions we had made.
At the end of the document, we give a summary of what were our biggest issues
and challenges, what we have learned out of that and what could have done
better.

## Technology Stack
The MiniTwit solution is only a small part of the system, which consists of
various tools and utilities that improve the development flow, monitoring,
logging and performance of the MiniTwit application. The following diagram, and
sections shows how these services fit together and form the architecture
surrounding the solution. The diagram shows the type of connections, as well as
any relevant comment to that connection.

![Deployment Diagram](./images/deployment_diagram.png)

The following sections seek describe what each of these do, with reasoning for
the inclusion. The chapter ends with an overview of the dependencies during
design-time, and how they relate to each other and external services implicitly
required for them to function. The chapter is structured in way that seeks to
describe the foundation of all our technologies and ending with external tools
used in the production environment of the application.

### Hosting

The application is required to have a publically available IP address. Therefore
we needed a hosting provider for the production environment. The course
presented [Digital Ocean][host-1] as an option, and as a couple of us had prior
experience with the service we choose this solution. The prior experience, along
with the option to utilize the [Github Student Developer Pack][host-2] for free
credits, meant we had confidence in that decision.

We started out provisioning a small [Droplet][host-3], which is what Digital
Ocean names their virtual private servers (VPS), with enough resources to host
the application and the database inside Docker containers. However, when we
added monitoring and logging, we had increasing requirements for the specs of
the VPS. To meet these requirements we would either have to provision multiple
Droplets or scale the existing VPS vertically would introduce downtime, which we
wanted to avoid.

We decided to have two droplets; one for our tools (e.g., logging) and
one for the solution. In hindsight this was probably the right decision as it
increases our resiliency. If one of the droplets crashes the other one remains
untouched. These will be referenced as _Application Droplet_ and _Tool Droplet_,
when applicable.

Despite the short-term benefit of this solution we still have issues scaling the
droplet hosting the application. As the application isn't stateless, horizontal
scaling isn't an option, and thus vertical scaling is our sole option, which
requires us to incur some downtime when the droplet is upgraded.

[host-1]: https://www.digitalocean.com/
[host-2]: https://education.github.com/pack
[host-3]: https://www.digitalocean.com/products/droplets/

### Operating System

For the operating system of the application droplet we decided on Ubuntu 18.04.3
LTS. It was important for us to use a version with long-term support (LTS) as it
helps us ensure stability and reliability as well as active support should bugs
or security vulnerabilties surfaced during the course.

We wanted to use a Linux based distribution as it seemed to provide the greatest
level of learning. Using Windows based virtual machine could provide us a
graphical user interface and tools, and it would definitely yield some valuable
learning as well, however it seemed to be a less attractive choice in the
context of this course and the tools we aimed to utilize. The choice of Ubuntu
is deliberate as we could have used Arch Linux or any other Linux distribution.
However, Ubuntu is the most common and has a great community, which make it
easier to debug and get support. These considerations combined made Ubuntu an
ideal choice with a group of varying levels Linux experience, and thus a more
limited level of proficiency with it.

### Containerization

To run the application and the database system, we decided on the use of
[Docker][container-1]. This choice rested primarily on it being presented in the
course, but a more important fact was that all group members had interest in
using the technology, and wanted some essential understanding of containers,
which has broad applicability in other similar technologies, for instance
Kubernetes.

There are other alternatives to Docker, but it is the primary technology
supporting containerization and thus an unoffical standard in the business. An
example of an alternative is [Vagrant][container-2] used to provision the
servers, but we deemed it less attractive as it is a rather heavy-weight
solution (i.e., entire operating system) in order to gain the same isolation
Docker provides.

We used [Docker Swarm][container-3] to horizontally scale the system. This
choice was primarily due to it providing all the features we required and
already being part of the Docker ecosystem, which we already had invested in. To
keep the setup simple we decided to run Docker Swarm with a single node (the
original host machine) acting as both the swarm manager and sole worker node. We
didn't invest much time looking into alternatives as Docker Swarm provided all
the tools necessary with less technical fragmentation, whereas an alternative
would require new configuration.


[container-1]: https://www.docker.com/
[container-2]: https://www.vagrantup.com/
[container-3]: https://docs.docker.com/engine/swarm/

### Programming language & Runtime environment

Before starting the refactoring of the existing MiniTwit application we
considered different programming languages, as well as the interests of the
group members regarding this decision. This of course had an impact on the
possibilities regarding web application frameworks.

We ended up using [.NET Core][prog-1] with C# as it was argued that it was the
language that most of the group members would be able to write from the start.
We wanted to focus less on the development of the application and more on
setting up the DevOps tools and processing related to it, so chosing a
completely new, and thus challenging language wasn't a priority.

The choice of the C# naturally led us to the usage of the [ASPNET Core][prog-2]
web framework.This framework provides us with good documentation on authoring
both server-rendered pages and REST APIs. For interaction with the database we
decided to use an ORM rather than handwritten SQL statements for reasons
regarding both security and speed of development. The choice of ORM ended on
[Entity Framework Core][prog-3] as it integrates very well with ASPNET Core, and
has adapters to many different databases, giving us freedom in the choice of
storage solution.

[prog-1]: https://dotnet.microsoft.com/
[prog-2]: https://dotnet.microsoft.com/apps/aspnet
[prog-3]: https://docs.microsoft.com/en-us/ef/core/

### Database
Initially the system utilized [SQLite][db-1], which was the original choice of
the application before refactoring. However, we wanted to use a more
full-fletched database in our production environment. This was motivated
primarily by the learning opportunity regarding the operation of a complex
database system in a production environment. On top of this we had a variety of
limitations regarding SQLite regarding query efficiency under load and lack of
features for scaling and backups.

We decided on the use of [Microsoft SQL Server][db-2]. This choice was
motivated by our prior investment into the .NET ecosystem, and the choice of
Entity Framework as our ORM solution. The ORM provided a freedom of storage
solution, however the ORM still sees the SQL Server a first-class supported
database as it also originates from Microsoft. The column data-types used in
T-SQL (which is the SQL dialect used in SQL Server) has direct translation to C#
types, which provides us with confidence in the reliability during
materialization of database record (e.g., not losing date-time or decimal precision).

We did consider other alternatives of relational databases, but ended up
deciding on the solution we had most confidence in. Alternatives like
[PostgreSQL][db-3] and [MySQL][db-4] were very similar, had similiar hardware
requirements, and provided no extra relevant functionality. We didn't spend time
looking into NoSQL solution as we wanted an easy approach when migrating data
from the existing SQLite database thus avoiding an ETL process of translating
the database schema into a NoSQL database. Lastly was the motivation that most
group members comfortable with relational databases.

The database itself, is mounted in a docker volume, to provide persistence even
upon a crash or restart, which normally would not be the case as containers are
non persistant.

[db-1]: https://www.sqlite.org/index.html
[db-2]: https://www.microsoft.com/en-us/sql-server/sql-server-2019
[db-3]: https://www.postgresql.org/
[db-4]: https://www.mysql.com/

### Monitoring

For the monitoring solution we decided on [Prometheus][mon-1] coupled with
Grafana for visualization. None of the group members had extensive prior
experience with monitoring tools, which meant that we had no preferences. Taking
a look at [Prometheus's own comparison to alternatives][mon-2] (granted that it
has a conflict of interests) made us comfortable that it would fit into the
setup we had planned. Primarily, Prometheus being designed to monitor metrics
over time, whereas some of the alternatives (e.g., [InfluxDB][mon-3]) is more
focused towards event logging, or has a less feature complete query languages.

Another aspect, which motivated this decision, was good community support along
with first-class support for [.NET based integration][mon-4] from Prometheus
themselves.

To ensure stability it was also important for us that Prometheus uses a
pull-based model when scraping metrics from the servers. The opposite solution
of a push-based solution could prove problematic as the amount of data could
overload the monitoring servers.

Integrating both Prometheus and Grafana proved to be simple due to the
availability of pre-built container images, which integrated nicely with our
investment into Docker.

[mon-1]: https://prometheus.io/
[mon-2]: https://prometheus.io/docs/introduction/comparison/
[mon-3]: https://www.influxdata.com/
[mon-4]: https://github.com/prometheus-net/prometheus-net

### Logging

We wanted to be sure we had an overview of the log messages produced by our
system, and any exception that might have occurred in production. This provides
two distinct problems, which will be covered here.

#### Event Logging
Event Logging is implemented using Elasticsearch, Logstash, and Kibana (often
called the ELK stack). The ELK stack is a popular choice to structured logging
with a large community supporting it. The community along with it being
presented during the course was the primary driving factors when deciding upon
this solution.

A common theme throughout the development cycle has been ease of integration and
the ELK stack did deliver in that area. However, we did research other
alternatives. This led us to looking more closely into
[LogDNA](https://logdna.com), which is Elasticsearch and Kibana combined. They
also support containerization, but suggests using Kubernetes, which we aren't
using. The integration story with .NET is also another negative for this
solution. We found a Github repository for a
[RedBear.LogDNA](https://github.com/RedBearSys/RedBear.LogDNA) library, but upon
further study it seemed to not be actively supported with few issues and pull
requests. And the issues there was mentioned deal-breaking issues related to
crashes of the application utilizing the library.

To send structured logs to Elasticsearch we used the .NET based library
[Serilog](https://serilog.net/) which is also a popular choice in the .NET Core
community, and it integrates very well with ASPNET Core and Entity Framework
Core using built-in logging interceptors gathering data without any additional
configuration.
#### Exception Logging
As mentioned in the start we also sought to gather exception from the production
environment. For this we ended up using [Sentry.io](https://sentry.io/welcome/).
This was motivated primarily from their ability to aggregate the exceptions, and
provide metrics with regards to the number of users affected by the exception,
but also from prior experience from some of the group members. This was a
relatively simple choice, and provided quick setup as well as an easy to read
interface. Additionally utilizing a managed solution, made sure that it wouldn't
crash or fail, which was crucial when considering the various bugs we would
accidentally introduce in our production environment. We could potentially
extract these from the ELK stack, but this was easy to setup and extract data
from, making it an ideal choice.

### Dependency Diagram 

Having been through all the technologies used in the application we end the
chapter with an overview of how these tools fit together.

The software solution had a variety of dependencies, both on various packages
and libraries, but also on external services that, would they crash, would crash
our service. The following diagram shows the relations between them and any
external services they rely on.

![Dependency Diagram](./images/dependency_graph.png)

A noticeable omission from the diagram is the technologies related to
containerization and the technologies supporting that (i.e., the operating
system). These has been omitted since they're a prerequisite of the entire
application, but doesn't play a role in the functionality of the application.
The use of containers does add some constraints on the architecture of the
application with regards to scaling (e.g., horizontal scaling requires
statelessness).

## CI/CD Pipeline

Having a strong CI/CD pipeline is a crucial development step in an agile
development team. Having automated deployment removes operations time from
having to manual deploy to production, it also limits potential errors.
Combining that with a variety of automated tests, makes sure that our production
environment is as close to perfect as possible. This, however, depends on the
quality of the tests and the various tools utilized.

This section will go through the choices we made relating to the pipeline, and
how it integrated in our workflow.

To understand the pipeline, it is first important to understand the context. The
following diagram illustrates a generic development flow.

![Development Flow](./images/development_diagram.png)

The pipeline is only a small part of the development flow, however, done
correct, it increases the effectiveness ten-fold.

### Continuous Integration

After any commit is pushed to the remote repository, the CI tests check if the
new feature introduces any regression, and if code quality is of satisfactory
levels. This is an important part of our review process of each feature - this
alleviated some pressure from the peer-reviewer, making it easier to trust that
the feature did not break anything - this cut down on time spent on reviews,
leaving room for developing features and operations.

#### Github Actions

Continuous integration required the ability to run a variety of scripts,
essentially, when ever a new commit enters the repository. We initially looked
at a variety of different CI possibilities, and considered Jenkins for one,
however we randomly looked at github and a group member asked if any of us have
every tried using Github Actions, their CI solution, and we realized that
neither of us had. This seemed like a great way to try something new and it also
seemed ideal with the current stack we were running. The choice was mainly based
on the availability and the possibility to learn something new.

We created three Github Action, whereas the following file is related to the
automated tests:

```YAML
name: Automated Tests

on: [push]

jobs:
  build:

    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v2
    - name: Setup .NET Core
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: 3.1.100
    - name: Run integration tests
      working-directory: ./src/
      run: docker-compose -f docker-compose.test.yml up --build --exit-code-from tester
    - name: Run unit tests
      run: dotnet test
      working-directory: ./src/WebApplication.Tests
```

#### Unit tests

To help increase our confidence in the changes added to the system we added unit
tests. The unit-tests are written in C# using the XUnit test framework, which is
used to test functionality as implemented in web application (e.g., creating a
user, adding a message, etc.).

The unit tests are focused around the service classes, which implements the
business logic related to the functionality of the system. These tests aims to
check the "happy-path" where the function succeeds as well as the expected
error paths (i.e., adding a message to an unknown user). The tests are executed
using the built-in tooling of the dotnet CLI included in the .NET Core SDK.

These tests are run as part of the CI pipeline on pull requests (PR), and when a
PR is merged into the master branch.

#### Integration tests

We wanted to test the API used by the simulator to make sure that this would
continue to work - Also as this surface is an API it is easier to test than a
graphical user interface, so we would be able to test all backend functionality
relatively easy.

The simulator that had to interface with our service was already written in
python3, so testing if the simulator would be able to work, would, in theory, be
as simple as running the simulator on a clean database, and see if the simulator
failed. It would still have to be rewritten a bit though.

As a member of our team has worked with python professionally for multiple years
he proposed he would convert it into unit tests. This made it somewhat easy to
isolate where errors occurred if something failed within this tester.

### Continuous Delivery

When new commits enter the master branch, we needs to update the production
server. The alternative would be to manually connect to the production
environment and pull the newest version of the system. This takes time and is
error prone, therefore this flow is automated. This is, in our case, done by
releasing a version of the code, organized by a docker image, to a package host,
which could then be downloaded by our production server.

**Github, as a packaging host**, has the possibility to host and provide
packages for downloading, much like Dockerhub. We considered moving our images
to Dockerhub, but having packages stored the same place as storing the
repository seemed ideal. We liked the idea of having a few select services that
we relied on, as to not create a too complex development flow, but rather
utilize few tools that integrated nicely together and provided each a large
chunk of the features required.

**Our github action**, automated the creation of the docker image as well as
pushing the image to the packaging host, seen
[here](https://github.com/Trivivium/devops-2k20/blob/master/.github/workflows/dockerpush.yml).
When pushed, the latest release can be accessed
[here](https://github.com/Trivivium/devops-2k20/packages/133732).

We had an [additional
action](https://github.com/Trivivium/devops-2k20/blob/master/.github/workflows/release.yml)
to create release [tags](https://github.com/Trivivium/devops-2k20/releases).

Upon the package being pushed to the packaging host, the next step is for the
production environment to pull the image.

**Watchtower** is a tool that automatically updates the base image of Docker
containers, which can be found [here](https://github.com/containrrr/watchtower).
There are a few different tools for this, and this is something Jenkins does
provide, however installing Jenkins for this feature alone, seemed overkill and
would use a magnitude more CPU power than we wanted. We wanted to utilize a
minimal tool that provides what was needed, and nothing else. This is what
watchtower did. It checks your package host with a consistant interval checks
whether the remote image has been updated - if it has, it updates the running
container. The setup was really simple, here showing the full configuration in
our `docker-compose`
[file](https://github.com/Trivivium/devops-2k20/blob/master/src/production.yml):

```yml
watchtower:
  image: v2tec/watchtower
  volumes:
    - /var/run/docker.sock:/var/run/docker.sock
    - /root/.docker/config.json:/config.json
  command: --interval 300
```

Worth noting, however, is that Watchtower does this by connecting to the docker
socket, making it able to spy on and interact with all running containers. This
is naturally dangerous if there is a security bug in watchtower, or if it is in
any way acts maliciously. We, however, trusted the service in the context of
this course, but that is a security estimate that should be considered in
projects.

The push step is easily visualized on the repository, making it transparent
whether the package creation was successful, however we have no way to monitor
whether the production is updated, nor the version of the software is running,
other than SSH'ing into the production server and checking.

## Development practices

With the Covid-19 epidemic emerging approximately half-way through the course,
the interaction amongst us inevitably changed as it was unwise to meet
physically. Initially when the course started, we would hold a meeting after the
lecture to plan the steps which needed to be accomplished for the following
weeks release.

As Covid-19 progressed, ITU closed down, and gatherings became rightfully
frowned upon which forced us to settle for the suboptimal approach of talking
over Zoom. While the content of these meetings were almost identical to that of
our physical meetings, the overhead of technical issues involving subpar
microphones and internet connections made these meetings far less efficient.
These meetings over Zoom took place approximately once per week. Meetings over
Zoom weren’t always necessary, which is why we also used Slack to keep each
other updated on progress on issues as well as to ask for consent to make
changes to the system or to ask for help.

The typical output of the meetings was an understanding of what tasks needed to
be carried out and by whom. These tasks would be posted as issues on Github,
where a group member would assign themselves to it. Using Github issues rather
than other tools, was an easy choice for us as it provided the functionality we
needed while also minimizing the spread of tools we used, seeing as we also used
Github for version control.

The vast majority of the issues were handled independently without pair
programming, which is definitely something we should have done in hindsight, as
mistakes could have been avoided and knowledge of more intricate details in our
program could be shared more conveniently. Although we didn’t define any clear
roles for the group members, the lack of pair programming resulted in some
intrinsic roles when the issues were perhaps larger than they should have been.
Being more consistent with creating smaller issues, could have solved this
problem, as multiple people would then have the opportunity of working on the
same subsystem.

When working on an issue that is some new feature in the project, we created a
new branch to work on it in a separate environment. Once the issue was deemed
complete, a pull request was made to finally merge the new feature into the
master branch. This branching strategy is very much in line with the ‘Topic
Branches’ model where branches are short-lived and become merged with the master
branch once the feature is working as intended. 
 
**Issue tracking / Kanban: Github** Generally speaking we never really put much
thought into how we would track issues and how we would separate the tasks at
hand. As we already had Github open, we simply created all our tasks on the
issue board there and never thought about alternatives. Alternatively we could
have created a Trello board or a Jira project, however with the limited scope of
the project it seemed extensive to include a whole other system just for task
management. As previously mentioned we generally tried limiting the number of
different tools we used, and create a stack with as few different tools as
possible.

## State of solution

_TODO - current state of our system, results of static analysis and code quality
assessment, add security assessment too_

## Evaluation

Here we list some of our biggest issues, lessons we have learned, overall
takeaways as well as some minor mistakes we had made that caused us troubles and
inconveniences.

### Application Development
During the development of the application we encountered a couple of issues that should
have been mitigated as researched beforehand, and a couple of noticeable features could
use some improvements to increase ease of development.

#### Use of SQLite during development
The first issue encountered was the use of SQLite for local development. Due to the overhead
of building and orchestrating containers every time the developer wanted to debug the
application we had intially decided to use SQLite. This choice worked well for the intended
purpose, but did prove to be a source of errors. The issue stems from the fact that SQLite
lacks some of the features related to constraints which the fully-fletched database server 
had. Thus when the developer tested any changes made the SQLite provider would be more
permissive than the production environment. We encountered this exact issue early on and
decided to move away from SQLite entirely by provisioning a MS SQL Server Docker image for
local development.

#### Database Migrations
The second issue we encounted was also related to the database. Using Entity Framework Core
provides us with the option of using Database Migrations to deploy database changes
automatically during start-up of the application. However, this requires us to opt-in to this
technology from the start, which we didn't. This had to consequence that database changes had
to be considered carefully, and was often worked around, as they would require manual intervention.
The key learning from this is the importance of having a plan for updates to the database
structure before launching the application in a production environment.

#### Hardcoded sensitive information
The third issue was reported as part of the security review of our system. The report pointed out
that we had sensitive information such as usernames/passwords in cleartext in the source code. This
involved components such as the database, logging etc. This was an embarrasing issue and the group
was in agreement that this should have been considered from the start. We decided on the solution
of storing the secrets using environment variables, where tools such as Docker secrets (for production)
and ASPNET Core secrets (for development) would provide means to store these separately from the
code.

### Operation

#### Containerization

The choice of manually installing, configuring, and operating Docker was a lot
more involved than a managed solution in terms of manual work, than many of the
managed alternatives, but it provided us with invaluable learning opportunities.
The setup process was interesting and we were able to learn various things about
the inner workings of docker, however it did leave space for the potential for
errors in critical components of the application, such as
[database deletion](https://github.com/Trivivium/devops-2k20/issues/50).

In accordance with our prior interest in Docker the choice of Docker Swarm was a
natural extension of this. The main hurdles encountered is the isolated knowledge
of the technology. Due to nature of it we allocated a single person to set it up,
which meant that when an error was encountered by others in the group we didn't
have a clear picture. However, this can be helped by documenting the approach
taken and sharing lessons learned.

Using a single node did have the consequence of reduced reliability and scaling
as we are restricted to the amount of resources on the host. However, both of
these concerns can be resolved with the additional physical nodes in the future,
which Docker Swarm simplifies greatly.

Due to hurdles and errors encountered, we would probably use a service provider
for this instead, in other cases. Be it Heroku, AWS or Azure, they all provide a
great ecosystem for these things, and reduce the risk of errors, and reduce
configuration time. This is naturally a tradeoff, and depends on the context in
which the development is taking place - having full control of your stack does
have it's advantages, however in projects of this size and type, it provides a
massive overhead.

With that being said, the choice was still great from a learning perspective.

#### Operating System

The operating system didn't seem to be crucial. The majority of our development
was in config files, and the challenges we would have required only a low level
Linux proficiency, however having an entry-level distribution did make it easier
to debug the various issues we would come across. Due to the technologies we
were planning on using (i.e., Docker) we weren't going to be working too much
directly on the operating system level. This meant that we didn't require to
have one of the group members focusing on the OS more than others. It also
proved to be a comfortable environment for the group members used to working in
Windows.

#### Exceptions
We didn't have exception logging from when the application initially launched.
This meant that we had a couple of days with downtime once in a while, without 
realizing it till it was too late. As mentioned previously we ended up utilizing 
Sentry.io to solve this problem. However we had already missed a lot of user
registrations when the simulator started. This had the consequence of us 
receiving many more errors due to the simulator attempting to create messages for
non-existing users. The key learning opportunity here is the importance of logging
unhandled exception in production environments.

The bulk of these exceptions occurred before the introduction of a service level 
agreement (SLA). However, considering the points in the SLA being important at any
stage in the process the exception did have the consequence of us breaking the terms
we defined. More specifically is the average number of errors pr. hour violated a few
times, and the mean recovery time is challanged too. This emphasizes the importance of
logging all errors from the start and tracking the rate of them.

#### Storing authentication signing keys inside containers
Due to the continuous delivery aspect of our deployment pipeline the running container
image was replaced often. This however had the side effect of wiping out any current
authenticated user sessions. The cause of this issue was due to the fact that signing
key used for authentication tokens was persisted in a directory inside the container. We
discovered this quite late in the process, but managed to solve this by moving the 
directory to a mounted Docker volume. 

The key learning here is the importance of knowing exactly what is stored inside the
containers, and testing the authentication works after deployment when we are working
with stateless sessions (which ASPNET Core is per default).

#### Storing database data inside containers
Much like the previous issue, we had an issue with the persistence of database data. 
The SQL Server image we used, store the data files of the database inside the container
per default. This wasn't an issue as long as the container wasn't removed since it was 
configured to persist during restarts. However, this did become an issue when moving to
Docker Swarm as that would provision a new container. 

The group solved this by moving the data files and transaction logs to a mounted Docker
volume as described in the official documentation for the image. The key learning here is
the importance of knowing how the database data is persisted, and options available to
handle this.

#### Losing the database volume when migrating to Docker Swarm
When we switched from normal Docker to Docker swarm mode the Docker engine didn't use the
same database volume as described above. This is due to the naming of volumes being
prefixed with the environment the container is run within (determined from the name of
the Docker Compose file), which we used. This meant that Docker created a new volume
with no data inside. Without active monitoring of our logging software we weren't fully 
aware that users were dropped and therefore didn't see the error. Also we didn't go
through the system thoroughly after the migration, so we didn't realize that
something was wrong until a few days later. We then had new users in the new
database, which meant that restoring from a backup would cause us to lose another
set of data.

The key learning here is the importance of creating database backups before large system
changes, and manually verifying that everything works as intended; Especially since we
didn't have any automated testing in place for this scenario. An other measure that would 
have helped was setting up chatops. Having the error log not being sent to a specific 
developer by email but rather in a chat we all had access too, would have helped. 
Additionally we could have monitored the monitoring and logs as well easily and created
various triggers. An example of a trigger would be monitoring the amount of 4xx 
HTTP responses that presumably would have increased afterwards.

#### Database Backups
As mentioned in the previous issue a key counter measure would have been the usage of
database backups. This is a persistence issue throughout the lifetime of the project
as backups had been manual, and only when we remembered to do it. The key learning 
opportunity here is the importance of the backup strategy before going into production.
This strategy would preferably being automatic, and be persisted at another host should
it crash with corruption of the storage.

Much related to this is the importance of the strategy actually testing the functionality
of the backups themselves before moving them to persistent storage. The initial database
created was located in the _master_ database of SQL Server, which is a system area not
able to backed up. This meant the first manual backup taken didn't work, and the group
had to move the database to a dedicated area and redo the backup process.

#### Disk space on server
As the number of requests and users from the simulator increased, we run out of
space, thus we missed some data. As a quick fix, we did a docker system prune
and successfully reclaimed more than 4GBs. After rescaling our system,
everything worked fine but we should have planned this in advance. This could been 
anticipated with more excessive infrastructure monitoring.

#### Continuous integration observations
Due to the continuous integration stage of our pipeline running integration tests
using an entire production environment (provisioned using Docker Compose) this 
process is time consuming. It's important to emphasize this hasn't been an issue,
but it should be seen as an observation of where to speed up the feedback to developers
when pushing changes to their pull requests.

The CI pipeline also currently creates a Github release before the solution has been built
and tested. This has introduced the chance of a release with errors being available. The
key learning opportunity here is the importance of ensure the different steps of the
pipeline runs in the correct logical order.

#### Github issues
We definitely had problems with our task-management and ended up doing some of
the tasks too late, so we definitely had to change our workflow, and would have
if we could do it over. I think the main issue was that we didn't consult the
issue list often enough, and possibly didn't put deadlines on, as well as not
assigning people to issues. Ideally we should probably have improved our overall
development process earlier on, but this is covered in the [Post
Mortem](../postmortem.md). We probably wouldn't have gotten any alternative
important features by choosing another service, as the problems we had were
based on structural team problems rather than the tool itself. Having the issues
closely aligned with the pull-request flow was definitely a helpful feature.

# Conclusion
ITU MiniTwit project gave us an excellent basis for learning and acquiring devops skills.
From system refactoring at the very beginning to writing technical documentation it
was challenging and very interesting experience. Working on it, we went through many
real life problems, both in devops area and team organization. We would like to emphasize
that this was a whole new experience to most of us. Regarding that, we gave our best efforts
in order to fulfill all the requirements every week. Sometimes, solutions were ad hoc, but
most of the time we approached systematically with clearly set procedures. We are aware that
our solution is not perfect, but we learned from our mistakes and that gave us the biggest value.
Last but not least, working in the environment like this, using cutting-edge tools and technology
helped and provided us with broad knowledge that we can start using immediately.