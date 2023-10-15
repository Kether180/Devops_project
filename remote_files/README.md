## MiniTwit

This project allows users to post and read tweets in real-time, and involves setting up a database and implementing various features. Docker containers are used to containerize both the database and the entire application. Additionally, the app can be monitored through Grafana/Prometheus, and it follows a DevSecOps methodology with continuous integration deployment via CI/CD GitActions.

The app has been deployed on Digital Ocean and has undergone a significant refactoring process, transitioning from a Python3 codebase Flask web framework to a C#/.NET Core React.js/JavaScript implementation. 

## Launching the project locally
To run the project locally, follow these steps:

## Run the Server Project using Docker Compose
docker-compose up --build
Go to your browser and open localhost:5050 or http://157.230.79.99:5050/

## Deploying database
The project uses various libraries such as sqlite3, dateTime, md5 Hashing, contextlib, and Render library. It also involves implementing methods such as Connect db, init db, and query db.

## Features
The project includes various user-specific features such as login/logout, register user, follow_user, unfollow_user, and add_message. It also includes timeline features such as get_user_timeline, get_public_timeline, and show_timeline.

## Subtasks
The subtasks involved in this project include setting up the database, setting up Digital Ocean, setting up CI/CD GitActions, and setting up Grafana/Prometheus.

## Metrics
The project includes a metrics endpoint that can be accessed at http://157.230.79.99:5050/metrics.

## Grafana
The project also includes a Grafana dashboard that can be accessed at http://157.230.79.99:3000/.

## Prometheus
The project also includes a Prometheus endpoint that can be accessed at http://157.230.79.99:9090.

## App UI

 => http://157.230.79.99:3001/