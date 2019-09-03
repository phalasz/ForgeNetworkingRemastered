# Forge Networking Remastered User Manual

### Here for the [Video Tutorials](https://www.youtube.com/watch?v=feLUmzaogCk&list=PLm1w78-UUlMIi5Vfwy6ckJQIQMHMT-QS5&index=3)?
[![ForgeYoutubeImageLink](http://img.youtube.com/vi/feLUmzaogCk/0.jpg)](https://www.youtube.com/watch?v=feLUmzaogCk&list=PLm1w78-UUlMIi5Vfwy6ckJQIQMHMT-QS5&index=3)

## Forge Networking Remastered (FNR) is a re-imagining of networking for Forge.
We took everything we learned, all the feedback we could get, and we started **from scratch** on FNR.

In short:
* While Forge is to be used as part of Unity like before, it is not dependant on Unity, which means you can even use it outside of Unity (in practical scenarios, to sometimes have C# servers that don't depend on Unity, but do act as servers to Unity Clients)
* We kept some of the helper classes such as **ObjectMapper** and **BMSByte**, as those were optimized classes just for generic network data transfer.
* We wanted to completely remove any kind of reflection "magic" that was in the system. In place of reflection, we opted for generated class files and code, this makes debugging and tracking execution much easier to maintain and test.

## Where to start
See the [Getting Started](GettingStarted/installation.md) docs for installation instructions and tutorials.

## Support & Questions
For anything related to Forge, there is a [community Discord!](https://discord.gg/yzZwEYm)
Everyone is **super** helpful and the entire community is in there, helping each other!
Even if you think your question is "silly" or super basic, a solution to it should arrive swiftly!!
Of course, it is more than just a place to ask questions ;)

## Contributing
First please familiarize yourself with the [Contribution Guidelines](https://github.com/BeardedManStudios/ForgeNetworkingRemastered/blob/master/CONTRIBUTING.md)

When you are done with that. Fork this repository to your own github account by clicking the `Fork` icon in the top right corner.

To start working on the new feature you would like to submit or to fix a bug create a new branch from the `develop` branch. When you are happy with the changes submit a [Pull Request](https://help.github.com/en/articles/about-pull-requests) (PR for short) to this repository. Please make sure to select the `develop` branch as the destination branch and your branch with the changes as the source.