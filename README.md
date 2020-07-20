# Crittr's Unity SDK

This package contains the SDK for Crittr's API in Unity.

### Installation
*Currently in Alpha and not available on the asset store. Will publish when a stable
release is ready*

#### Install SDK locally

1. Clone or download and extract the repository to a local folder (e.g. `C:\Downloads\Crittr\unity-sdk`).

2. Open up Unity using an exising project or create a new one.
    * In Unity, open the package manager (`Window > Package Manager`)
    * From the package manager, click the `+` icon on the top left and select the option `Add package from disk...`
    * Navigate to the downloaded repository and select the `package.json` file. (e.g. `C:\Downloads\Crittr\unity-sdk\package.json`)
    * Open the `package.json` file and the SDK should install.

### Using the SDK

#### Create the config file.

1. In your project's top level `Assets` folder, create a new folder `Resources` (if one doesn't already exist).

2. In the `Resources` folder, right click to open the asset menu and select the `Crittr Config` option.
    * A new file named `CrittrConfig` should have been created in the `Resources` folder.
    * Left-click to inspect the `CrittrConfig` properties.
    * Input the Connection URI for your project (you can find this in the project settings SDK section on [Crittr's dashboard](https://dashboard.crittr.co))

#### Sending your first report

A default SDK class exists to help send your first report. You can use this class in the following way:

1. Create an empty game object in your Scene.

2. Attach the CrittrSDK script component to the game object (make sure this game object doesn't get destroyed while running the game!).

3. Run your game and then press `F8`. A screenshot of your game will be taken and Unity should then open up your default browser directing you to the created report update page in the Crittr dashboard.

4. Update the report with a new title and description.

5. That's it! You should be able to see a screenshot and the report in the reports list of the project.
