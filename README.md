# Crittr's Unity SDK

This package contains the SDK for Crittr's API in Unity.

### Installation
*Currently in Beta and not available on the asset store. Will publish when a major
release is ready*

#### Install the SDK

#### Install from Github

1. Use the Github url for the master branch, or the latest release found in the releases tab. 

2. Follow the instructions from the [official Unity documentation](https://docs.unity3d.com/Manual/upm-ui-giturl.html) to install the SDK.

#### Install locally

1. Clone or download and extract the repository to a local folder (e.g. `C:\Downloads\Crittr\unity-sdk`).

2. Open up Unity using an exising project or create a new one.
    * In Unity, open the package manager (`Window > Package Manager`)
    * From the package manager, click the `+` icon on the top left and select the option `Add package from disk...`
    * Navigate to the downloaded repository and select the `package.json` file. (e.g. `C:\Downloads\Crittr\unity-sdk\package.json`)
    * Open the `package.json` file and the SDK should install.

### Using the SDK

1. In your project's top level `Assets` folder, create a new folder `Resources` (if one doesn't already exist).

2. In the `Resources` folder, right click to open the asset menu and select the `Crittr Config` option.
    * A new file named `CrittrConfig` should have been created in the `Resources` folder.
    * Left-click to inspect the `CrittrConfig` properties.
    * Input the Connection URI for your project (you can find this in the project settings SDK section on [Crittr's dashboard](https://dashboard.crittr.co))

#### Sending your first report

A default CrittrReporter and CrittrCanvas prefab exist in the `Assets/Prefabs` directory to help send your first report. To use them:

1. Add the prefabs to your Scene.

2. Add the Connection URI to the CrittrReporter prefab:
    * Scroll to the CrittrSDK script.
    * Input the Connection URI for your project (you can find this in the project settings SDK section on [Crittr's dashboard](https://dashboard.crittr.co))

3. Run your game and then press `F8`. A screenshot of your game will be taken and a success screen should pop up with a link and QR code to update your report.
    * If a screen does not pop up, check the logs to see if you have inputted the connection uri correctly.
    * If the failure screen shows, check that you have a valid connection uri.

4. Update the report with a new title and description.

5. That's it! You should be able to see a screenshot and the report in the reports list of the project.
