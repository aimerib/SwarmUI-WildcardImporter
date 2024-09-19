# Wildcard Importer Extension for SwarmUI


**Wildcard Importer** is a powerful extension for [SwarmUI](https://swarmui.com/) designed to streamline the process of importing and managing wildcards downloaded from [Civitai](https://civitai.com/), this extension enables batch processing, parsing, and organization of wildcards into SwarmUI's designated wildcards folder with ease.

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
- [Example Outputs](#example-outputs)
- [License](#license)


## Features

- **Batch Importing**: Easily import multiple wildcards in one go using structured YAML files.
- **Wildcard/Random Replacement**: Recognizes and replaces wildcard and random directives to integrate seamlessly with SwarmUI's wildcard manager.
- **User-Friendly Interface**: Intuitive UI with drag-and-drop support for uploading YAML files.

## Installation

1. **Download the Extension**

   Clone the latest version of the Wildcard Importer extension from the [Github Repo](https://github.com/aimerib/SwarmUI-WildcardImporter/) page, or download the latest release from the [Releases](https://github.com/aimerib/SwarmUI-WildcardImporter/releases) page.

2. **Extract the Files**

   Unzip the downloaded archive to a desired location on your machine if you downloaded the zip file.

3. **Place in SwarmUI Extensions Folder**

   Move the extracted `SwarmUI-WildcardImporter` folder to SwarmUI's extensions directory, typically found at:

   ```
   /path/to/SwarmUI/src/Extensions/
   ```

4. **Restart SwarmUI**

   After placing the extension, restart the SwarmUI application to load the new extension.

5. **Activate the Extension**

   Navigate to SwarmUI's extensions management panel and ensure that the Wildcard Importer extension is activated.

## Usage

Download all your wildcards from Civitai and put them in a folder. Open the Wildcard Importer extension in SwarmUI and drag and drop the folder containing your wildcards into the extension window, or click on "Select Files" and click on the "Process Wildcards" button. The wildcards will be processed and added to your SwarmUI.

### Example outputs

Take this example YAML file:

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

**Import via UI**

- Navigate to the **Wildcard Importer** tab within SwarmUI.
- Drag and drop your `.yaml`, `.txt`, or `.zip` files into the designated dropzone or click to select the files manually.
- Multiple files can be selected at once.
- Click on the **Process Wildcards** button to initiate the import.
- Monitor the **Processing Status** panel for real-time updates.

**Resulting Folder Structure**

After successful processing, the following structure will be created in the `ponyxl-jobs` folder:

```
ponyxl-jobs/
│   (top level wildcard containing all scifi jobs as <wildcard:mobile_trooper>, <wildcard:netrunner>, etc.)
├── scifi_jobs.txt
├── scifi_jobs/
│   │  (contains all the lines for the mobile_trooper wildcard)
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

Consider the following example:

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
After successful processing, the following structure will be created in the `BoHalloween`  and `halloween` folders:

```
BoHalloween/
├── random-location.txt
(contains all the lines for the random-location wildcard, in the example above it would be <wildcard:halloween/Places-Sceneries>, <wildcard:halloween/Atmosphere>, <wildcard:halloween/Decorations>, <wildcard:halloween/Symbols-Icons>, <wildcard:halloween/Lighting>, <wildcard:halloween/ArtStyles>)
├── random-ppl.txt
(contains all the lines for the random-ppl wildcard, in the example above it would be <wildcard:halloween/Costumes>, <wildcard:halloween/Atmosphere>, <wildcard:halloween/Food-Treats>, <wildcard:halloween/Lighting>, <wildcard:halloween/Symbols-Icons>, <wildcard:halloween/Grimaces>, <random:<wildcard:halloween/Symbols-Icons>,|<wildcard:halloween/Traditions>,|<wildcard:halloween/Symbols-Icons>,|<wildcard:halloween/Traditions>,|)
└── ... other random categories
halloween/
├── Atmosphere.txt
├── Decorations.txt
└── ... other categories
```


## License

This project is licensed under the [MIT License](LICENSE).

---

*Happy Wildcarding! 🚀*

---
