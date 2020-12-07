# What's Koasta?

Koasta is an open source digital ordering platform for bars and restaurants. It consists of:

- a behind the bar POS interface for managing digital orders
- an iOS and Android app for end users to place orders with
- an online dashboard to manage menus, products, reviews, and more

# How do I use this?

## Koasta as a service

We offer a managed service version of Koasta with a monthly subscription where we manage the day to day running of your personal Koasta instance. [Get in touch](mailto:hello@meshtech.io) to find out more.

## Custom deployment

You're free to run Koasta for free by building and deploying Koasta to the infrastructure of your choosing. We offer paid support services if you need help getting things running or maintaining the service over time. [Get in touch](mailto:hello@meshtech.io) to find out more.

Koasta has been run in a wide variety of environments including kubernetes clusters, AWS and Azure App Services. This means Koasta is very flexible about where it's run. You'll need the following infrastructure for the platform to function:

- A RabbitMQ cluster (used for all order processing and push notifications)
- A PostgreSQL database with a few extensions installed (see [local_db_setup.sql](scripts/docker/sql/local_db_setup.sql) for more details)
- A Memcached cluster (used for feature flags e.g. Facebook auth)

You have two options for how you run Koasta - as a vertically scaled single instance via the Monolith project, or a horizontally scaled series of services via each individual service folder. It's strongly recommended you go with the latter approach and have a large number of event service instances in order to ensure high order throughput during peak hours.

You'll need to update anything with the value `CHANGEME`. Various bits and bobs need setting up including but not limited to:

- An iOS developer account and Apple Pay merchant ID
- A Play Store developer account and Google Pay pre-approval
- A Firebase account for Android push notifications
- An iOS Push key
- A Square developer account for transaction processing
- A Facebook developer account for Facebook social auth

Both Google and Facebook authentication on Android require signing keys to be exported from your Google Play developer account and added to the appropriate companies' systems, as otherwise you'll get authentication errors. Google replaces your keystore's signing key with its own, which may trip you up, so make sure you have the correct signing keys in place to avoid this. iOS does not exhibit this issue.

Service charges can be configured by setting the 'Koasta fee' within the Order Service's [configuration](services/Order/config). If the Square fees no longer match the order estimates, you can update the percentages here also.

From a legal standpoint, the Koasta brand and name are copyright Mesh Services Limited. You should replace all branding to be appropriate for your company for both mobile applications. The Koasta names and branding are reserved on both app stores for Mesh Services Limited's exclusive use.
