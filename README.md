# `atc-net.github.io` is the website for `ATC-NET` organization

The purpose of this website is to provide an overview of all ATC-NET's repositories in one landing page - visit [ATC-NET's website](https://github.com/atc-net)

This means that 98% of all content on this website is in the various repositories and is just collected and then rendered on this website.

## Technology selection and architecture setup

The `atc-net.github.io` website is build in Blazor WebAssembly with the UI framework MudBlazor.

When commiting a PR to the `atc-net.github.io` repository, a build and release pipeline will ensure a compiled Blazor webassembly will be publish in a github-pages (known as the reserved gh-pages branch name).

## Q & A

### Q: Why is the repository called `atc-net.github.io`

> GitHub magic - a feature in GitHub, that takes the organization name `atc-net` plus `.github.io` and combined it to serve a free DNS record. [Read more on Creating a GitHub Pages site](https://docs.github.com/en/pages/getting-started-with-github-pages/creating-a-github-pages-site)

### Q: Why is content not updated when a another repository commit just done

> The website use caching to avoid DDoS attack against github.com. So data collecting request go throu a proxy API (`atc-api`) to collect data.
<br />
> The default cache expiration time is set `24 hour`.

## How to contribute

[Contribution Guidelines](https://atc-net.github.io/introduction/about-atc#how-to-contribute)

[Coding Guidelines](https://atc-net.github.io/introduction/about-atc#coding-guidelines)
