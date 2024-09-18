# Wildcard Importer Extension for SwarmUI


**Wildcard Importer** is a powerful extension for [SwarmUI](https://swarmui.com/) designed to streamline the process of importing and managing wildcards downloaded from [Civitai](https://civitai.com/), this extension enables batch processing, parsing, and organization of wildcards into SwarmUI's designated wildcards folder with ease.

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
  - [Processing `ponyxl-jobs.yaml`](#processing-ponyxl-jobsyaml)
  - [Processing `halloween.yaml`](#processing-halloweenyaml)
- [Supported YAML Structures](#supported-yaml-structures)
- [API Endpoints](#api-endpoints)
- [Folder Structure Output](#folder-structure-output)
- [Contributing](#contributing)
- [License](#license)
- [Support](#support)

## Features

- **Batch Importing**: Easily import multiple wildcards in one go using structured YAML files.
- **Custom Parsing**: Manually parses YAML without relying on external libraries, ensuring compatibility within the SwarmUI extension environment.
- **Wildcard Replacement**: Recognizes and replaces wildcard patterns to integrate seamlessly with SwarmUI's wildcard manager.
- **Conflict Handling**: Provides mechanisms to resolve file conflicts during the import process.
- **Undo Operations**: Ability to revert the last import operation if needed.
- **Processing History**: Maintains a history of all import tasks for easy tracking and management.
- **User-Friendly Interface**: Intuitive UI with drag-and-drop support for uploading YAML files.

## Installation

1. **Download the Extension**

   Obtain the latest version of the Wildcard Importer extension from the [Releases](https://github.com/aimerib/wildcard-importer/releases) page.

2. **Extract the Files**

   Unzip the downloaded archive to a desired location on your machine.

3. **Place in SwarmUI Extensions Folder**

   Move the extracted `WildcardImporter` folder to SwarmUI's extensions directory, typically found at:

   ```
   /path/to/SwarmUI/extensions/
   ```

4. **Restart SwarmUI**

   After placing the extension, restart the SwarmUI application to load the new extension.

5. **Activate the Extension**

   Navigate to SwarmUI's extensions management panel and ensure that the Wildcard Importer extension is activated.

## Usage

Download all your wildcards from Civitai and put them in a folder. Open the Wildcard Importer extension in SwarmUI and click on the "Import Wildcards" button. Select the folder containing your wildcards and click on the "Process Wildcards" button. The wildcards will be processed and added to your SwarmUI.

### Example outputs

1. **Prepare Your YAML File**

   Ensure your `ponyxl-jobs.yaml` is structured as follows:

   ```yaml
   ponyxl:
     chara_jobs:
       scifi_jobs:
         mobile_trooper:
           - scifi, mecha musume, helmet, huge gauntlets, gray mobile suit, exoskeleton, heads-up display, visor,
         netrunner:
           - cyberpunk, high_tech_gear, visor, holographic clothing, hood down, black jacket,
         # ... other scifi jobs
       modern_jobs:
         part-timer:
           - lanyard, red cap, fast food uniform, red skirt, collared_shirt, t-shirt, visor cap, mcdonald's,
         racer:
           - race queen, formula racer, red jumpsuit, helmet,
         # ... other modern jobs
   ```

2. **Import via UI**

   - Navigate to the **Wildcard Importer** tab within SwarmUI.
   - Drag and drop your `ponyxl-jobs.yaml` file into the designated dropzone or click to select the file manually.
   - Click on the **Process Wildcards** button to initiate the import.
   - Monitor the **Processing Status** panel for real-time updates.

3. **Resulting Folder Structure**

   After successful processing, the following structure will be created in the `ponyxl-jobs` folder:

   ```
   ponyxl-jobs/
   ├── scifi_jobs.txt
   ├── scifi_jobs/
   │   ├── mobile_trooper.txt
   │   ├── netrunner.txt
   │   └── ... other scifi job categories
   ├── modern_jobs/
   │   ├── part-timer.txt
   │   ├── racer.txt
   │   └── ... other modern job categories
   └── ... other primary categories
   ```

   Each `.txt` file contains the respective job entries, formatted as comma-separated values.

### Processing `halloween.yaml`

1. **Prepare Your YAML File**

   Ensure your `halloween.yaml` is structured as follows:

   ```yaml
   BoHalloween:
     random-location:
       - __halloween/Places-Sceneries__, __halloween/Atmosphere__, __halloween/Decorations__, __halloween/Symbols-Icons__,
         __halloween/Lighting__, __halloween/ArtStyles__
     random-ppl:
       - "__halloween/Costumes__, __halloween/Atmosphere__, __halloween/Food-Treats__, __halloween/Lighting__,
         {__halloween/Symbols-Icons__,|__halloween/Grimaces__,|__halloween/Symbols-Icons__,|__halloween/Traditions__,|}"
     # ... other random categories
   halloween:
     Atmosphere:
       - Bewildering
       - Bewitched
       - Bloodcurdling
       - Bone-chilling
       - Chilling
       - Creepy
       - Cryptic
       - Cursed
     Decorations:
       - Bats
       - Black Cats
       - Candelabras
       - Cauldrons
       - Cobwebs
       - Coffin Props
       - Crystal Balls
     # ... other categories
   ```

2. **Import via UI**

   - Navigate to the **Wildcard Importer** tab within SwarmUI.
   - Drag and drop your `halloween.yaml` file into the designated dropzone or click to select the file manually.
   - Click on the **Process Wildcards** button to initiate the import.
   - Monitor the **Processing Status** panel for real-time updates.

3. **Resulting Folder Structure**

   After successful processing, the following structure will be created in the `BoHalloween`  and `halloween` folders:

   ```
   BoHalloween/
   ├── random-location.txt
   ├── random-ppl.txt
   └── ... other random categories
   halloween/
   ├── Atmosphere.txt
   ├── Atmosphere.txt
   ├── Decorations.txt
   └── ... other categories
   ```

   - **Wildcard Files**: Files like `random-location.txt` will contain wildcard placeholders, e.g., `<wildcard:halloween/Places-Sceneries>`.
   - **Categorical Files**: Files like `Atmosphere.txt` will list individual attributes related to the category.


## License

This project is licensed under the [MIT License](LICENSE).

## Support

If you encounter any issues or have questions regarding the Wildcard Importer extension, please open an issue on the [GitHub Issues](https://github.com/your-repo/wildcard-importer/issues) page or contact the maintainer at [your-email@example.com](mailto:your-email@example.com).

---

*Happy Wildcarding! 🚀*

---
