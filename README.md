![Deployment](https://github.com/instilledbee/the-ultimate-nerd/actions/workflows/deploy.yml/badge.svg)

# the-ultimate-nerd
My personal programming blog. Powered by [Statiq](https://statiq.dev).

See it live [here](https://instilledbee.net/blog).

Building
--------
TODO: Create a detailed blog post for this

* Clone this repository, ensuring submodules are included
* Run `dotnet run` to restore the latest Statiq Nuget package and generate the output files
* Run `dotnet run preview --virtual-dir blog` to preview the files at `http://localhost:5080/blog`
  * For more details, see instructions in [Statiq Preview Server](https://www.statiq.dev/guide/running/preview-server)
