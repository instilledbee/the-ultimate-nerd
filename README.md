# the-ultimate-nerd
My personal programming blog. Powered by [Wyam](https://github.com/Wyamio/Wyam/) See it live [here](https://instilledbee.net/blog).

Building
--------
TODO: Create a detailed blog post for this

* Download [Wyam](https://wyam.io)
* Run `build.ps1` to install Cake dependencies
* Use `build.cake` to Build, Preview, or Deploy

Cake script parameters
---------------------
* `target` - the command to run:
  * "Build" - runs **Wyam** to transform the files in the `input` directory to assets in the `output` directory
  * "Preview" - runs **Wyam** to perform a **Build** and then spins up a local server to preview the `output`. By default, the preview runs on `http://localhost:5080/blog`
  * "Deploy" - runs **Wyam** to perform a **Build** and then uploads to an FTP server (see below)

* `username` - the FTP username to log in as when running **Deploy**
* `password` - the FTP password to use when running **Deploy**
* `path` - the absolute path on the server to begin uploading from. The directory structure of the `output` folder will be copied relative to the specified path.

Examples:
* `.\build.ps1 --target="Build"`
* `.\build.ps1 --target="Deploy" --username="ftpUsername" --password="soSecure123" --path="ftp://awesome.blog/pls_upload_here`