!-- IMPORTANT: This document is for recruiment purposes only. We kindly ask that you do not share, copy, or distribute it without permission. -->
Intro

A new development project has started for a large industrial customer whose operations require careful planning of resource usage. The project is called "Resource Allocation Management System".
Current system

The customer's current system is severely outdated, difficult to maintain and it has not been possible to integrate it to the newer cloud-based systems. The customer has decided to invest in development of a new cloud-based system and use this opportunity to rethink design and technical decisions.

The main responsibilities of the system include:

    Allocating personnel and machinery (resources) to factories.
    Scheduling reservations for resources.
    Overview of allocated resources by type and location.
    The permissions are role based: managers can update personnel and factory information, while employees are only allowed to allocate resources.

Project status

The customer would prefer to see a technical proof of concept (PoC) for the new system as soon as possible. The purpose for the PoC is to aid in the decision making process, clarify budgeting and get internal buy-in. What we know so far:

    The system should be cloud-based web application running in company's own cloud provider tenancy.
    The company will later provide specifications for the visual style and UI implementation once the system's specifications have been decided on, while also taking the findings provided by the PoC into account.
    Databases are used for persistent data, but the type of the database is unknown.
    Frontend framework has not been decided yet. The customer trusts the development team to make the decision with appropriate reasoning.
    The language for the backend has not been decided. The customer's technical team is familiar with .NET and NodeJS, but is open for other options as well.

Your role in the project

Your task is to implement the PoC and join the next project meeting and present it to the customer in order to kickstart the discussion.
Assignment details

Your assignment is to create the PoC discussed in earlier chapter. The assignment is completed by implementing as many of the already decided functionalities (see the task list below) as you can.

The returned assignment will be discussed in the technical review. The purpose is to provide a way to demonstrate your technical skills and decision making process. You are not expected to complete every task in the indended schedule for this assignment.

We are also interested about your ability to explain your work and decisions you have made. In addition, we are curious what changes or improvements you would make going forward if you were given more time.

The outcome of the assignment cannot be "finished" or perfect solution and it is not reasonable to expect such. It should, however, be in executable state.

Deliverable:

    Your implementation of the assignment. We kindly ask you to not share the assignment via public and discoverable links.

Practical tips:

    Do not spend more than a couple of evenings for this assignment.
    The code is not expected to be perfect (it cannot be).
    You can freely decide which tasks to implement and in what order. We are interested to hear what led you to these decisions though!
    Using AI tools is not prohibited, but you are expected to point out where and why they were used during the interview.
    Focus on the big picture and aim to get as much work done as possible instead of getting stuck in the details.

Evaluation criteria:

    Clear instructions on how to run your app.
    You are able to explain your decisions.
    You are able to explain technical nuances of your solution, if needed.
    Code quality (clean, readable, and not over-engineered).

Tasks

Choose the tasks you would like to work on. You are free to choose how you implement them and they don't have to be completed to the same level of detail.

1. Factory information (high priority)

System should allow users to manage factory information.

    Factory name - Unique, not empty
    Factory time zone - IANA TimeZone identifier, not empty.

2. Personnel information (high priority)

System should allow users to manage personnel information.

    Personal ID - Unique, not empty.
    Full name - Not empty.
    Email - Unique, not empty.
    List of factories where this person can be allocated.

3. Reservations (high priority)

System should allow users to manage personnel reservations.

    Each reservation includes a factory where the person is allocated to.
    Each reservation includes at least one person.
    Each reservation requires a start and an end time (date and time, in a format of your choosing)
    A person can only be allocated to one factory at any moment of time, but can otherwise have multiple reservations.
    A person can only be allocated to a predefined list of factories (see task #2).

4. Scheduling overview (high priority)

System should provide users with an overview of personnel reservations.

    Per person - Reservations including their durations (in hours).
    Per factory - Reservations including sum of hours from all the reservations for the factory.

5. Authentication and authorization plan (low priority)

   System should support different user roles to limit system access for non-administrative users.
   Instead of implementing, you can plan and describe how you would approach authentication and authorization, and how these would affect the other tasks in this list.

6. Additional Requirement from Bao

   When deleting a Personnel, all Reservations associated with that will not be delete but set the personnel field to null or "Deleted Personnel".
   When deleting a Factory, all Reservations associated with that will not be delete but set the factory field to null or "Deleted Factory".
