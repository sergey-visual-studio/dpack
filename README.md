[![Build Status](https://dev.azure.com/usysware/dpack/_apis/build/status/usysware.dpack?branchName=master)](https://dev.azure.com/usysware/dpack/_build/latest?definitionId=1&branchName=master)
![Azure DevOps tests (branch)](https://img.shields.io/azure-devops/tests/usysware/dpack/1/master)

# DPack is in BETA!

Please be aware that DPack Rx currently is and will remain for a while in **BETA** status. Tread carefully and report any issues you might find. Thanks!

Areas where I could use some help:
- Testing the latest beta on VS 2017 and 2019
- Graphics of DPack in action for this readme
- Graphics for DPack's menu items

### DPack Rx (former DPack)

FREE tools collection designed to greatly increase developer's productivity, automate repetitive processes and expand upon some of Microsoft Visual Studio features. Visual Studio 2017 and 2019 are supported.

### What is DPack Rx?

DPack Rx is an effort to remedy former [DPack](https://marketplace.visualstudio.com/items?itemName=SergeyM.DPack-16348) limitations: bring it under OSS umbrella, upgrade it for more modern Visual Studio integration, and apply more modern technology and development practices. I also felt the effort needed a new product name.

Driving principal behind DPack design is fire-and-forget user experience with minimal impact on the development environment. To adhere to that principal most of the major features are invoked on demand, present a dialog (if applicable), which goes away once user interaction ends. Little to no impact on the development environment is expected from thereafter. Taking all that into account, features such as ToolWindow support don't fit well into that design paradigm.

### What features are included so far?

-	*File Browser* feature
-	*Code Browser* feature
-	*Bookmarks* feature

### Future plans

- ~~CI pipeline setup~~
- ~~Publish to Visual Studio Gallery~~
- Setup Wiki page
- Migrate former DPack *Solution Backup* feature
- Migrate former DPack *Solution Statistics* feature

### Major changes

- Both *File Browser* and *Code Browser* features UI has been revamped completely in WPF
- *Bookmarks* feature has been rebuilt from the ground using editor taggers
- *Bookmarks* feature bookmarks are no longer saved with the solution. This might be subject to change in the future
- *Bookmarks* feature ToolWindow's been deprecated
- Former browsers *ToolWindow Mode* is no longer available (see the note above on design principals)
- *Solution Browser* rather infrequently used feature won't be migrated. If you rely on it then consider staying with older DPack extension
- *Surround With* feature won't be migrated either
- *Code Navigation* obscure feature won't be migrated either

Thanks and enjoy.

Sergey @ USysWare
