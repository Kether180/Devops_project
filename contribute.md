- Which repository setup will we use?

We have decided to use github for our repo management. This is something we were encouraged to use and a free service that we also had experience with in advance.
We have chosen a mono-repo approach as the project is not going to be so big that more repositories would be necessary. There could be arguments for a dual repo setup with a repo for the back-end and another for the front-end but sticking to a single made more sense as the arguments for a poly-repo setup are not strong enough in the case of 2 repos over a single.

- Which branching model will we use?

We will create branches according to every described issue in our backlog. Furthermore we have decided to split our branches into 3 categories:

Main: Used for releases and will always have a stable version of the website for   releasing

Development: Is the newest version of our website. Will contain the latest, tested features

fix/feature: Will contain individual changes according to our backlog

At the start we also had a 4th release branch that was made redundant later. 

- Which distributed development workflow will we use?

We have decided to split up our work throughout the group. The group members will have different responsibilities for parts of the website such as frontend, backend etc.
How do we expect contributions to look like?
A contribution should only be allowed into the development branch if the code is written in a readable manner, is tested and solves an issue contained in the backlog. At the moment contributions can only be made by the group. The contributions also has to pass the github actions checks unless there could be made arguments for a failing check is redundant.

- Who is responsible for integrating/reviewing contributions?

Every member in the group is responsible for reviewing contributions but a contributor will request reviews from team members more knowledgeable about the code and issues.
