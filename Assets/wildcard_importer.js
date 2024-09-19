document.addEventListener("DOMContentLoaded", function () {
  initializeWildcardImporter();
  Dropzone.options.wildcardImporterDropzone = {
    // Configuration options go here
    autoProcessQueue: false,
    uploadMultiple: true,
    parallelUploads: 100,
    maxFiles: 100,
    url: "",
    init: function () {
      var myDropzone = this;

      // First change the button to actually tell Dropzone to process the queue.
      this.element
        .querySelector("button[id=wildcardImporter-process-wildcards]")
        .addEventListener("click", function (e) {
          // Make sure that the form isn't actually being sent.
          e.preventDefault();
          e.stopPropagation();
          myDropzone.processQueue();
        });

      // Listen to the sendingmultiple event. In this case, it's the sendingmultiple event instead
      // of the sending event because uploadMultiple is set to true.
      this.on("sendingmultiple", function () {
        // Gets triggered when the form is actually being sent.
        // Hide the success button or the complete form.
      });
      this.on("successmultiple", function (files, response) {
        // Gets triggered when the files have successfully been sent.
        // Redirect user or notify of success.
      });
      this.on("errormultiple", function (files, response) {
        // Gets triggered when there was an error sending the files.
        // Maybe show form again, and notify user of error
      });
    },
  };
});

function initializeWildcardImporter() {
  const dropzone = document.getElementById("wildcardImporter-dropzone");
  const fileInput = document.getElementById("wildcardImporter-file-input");
  const processButton = document.getElementById(
    "wildcardImporter-process-wildcards"
  );
  const undoButton = document.getElementById(
    "wildcardImporter-undo-processing"
  );
  const statusDiv = document.getElementById(
    "wildcardImporter-processing-status"
  );
  const historyDiv = document.getElementById(
    "wildcardImporter-processing-history"
  );

  let files = [];

  // Handle click on dropzone to trigger file input
  dropzone.addEventListener("click", () => fileInput.click());

  // Handle dragover event to provide visual feedback
  dropzone.addEventListener("dragover", (e) => {
    e.preventDefault();
    dropzone.classList.add("dragover");
  });

  // Handle dragleave event to remove visual feedback
  dropzone.addEventListener("dragleave", () => {
    dropzone.classList.remove("dragover");
  });

  // Handle drop event to capture files
  dropzone.addEventListener("drop", (e) => {
    e.preventDefault();
    dropzone.classList.remove("dragover");
    files = Array.from(e.dataTransfer.files);
    updateDropzoneText();
  });

  // Handle file selection via file input
  fileInput.addEventListener("change", () => {
    files = Array.from(fileInput.files);
    updateDropzoneText();
  });

  // Handle process wildcards button click
  processButton.addEventListener("click", () => {
    if (files.length > 0) {
      processWildcards(files);
    } else {
      alert("Please select files to process.");
    }
  });

  // Handle undo processing button click
  undoButton.addEventListener("click", undoProcessing);

  // Update dropzone text based on selected files
  function updateDropzoneText() {
    dropzone.textContent =
      files.length > 0
        ? `${files.length} file(s) selected`
        : "Drop files here or click to select";
  }

  // Initialize destination folder and processing history
  updateDestinationFolder();
  updateHistory();
}

/**
 * Processes wildcard files by sending their Base64-encoded content to the backend.
 * @param {FileList | File[]} files - An array or FileList of File objects to process.
 */
async function processWildcards(files) {
  try {
    // Convert the FileList or Array of Files to an Array and map each to a FileData object
    const fileDataArray = await Promise.all(
      Array.from(files).map(async (file) => {
        const base64Content = await readFileAsBase64(file);
        return {
          FilePath: file.name, // Using file.name as the FilePath
          Base64Content: base64Content,
        };
      })
    );

    // Serialize the array of FileData objects into a JSON string
    const filesJson = JSON.stringify(fileDataArray);

    // Prepare the data payload with the serialized JSON string
    const payload = { filesJson };

    // Send the payload to the 'ProcessWildcards' API endpoint
    genericRequest(
      "ProcessWildcards",
      payload,
      (data) => {
        if (data.success) {
          alert("Processing started. Task ID: " + data.taskId);
          updateStatus(data.taskId);
        } else {
          alert("Error: " + data.message);
        }
      },
      true
    );
  } catch (error) {
    console.error("Error processing wildcards:", error);
    alert("An error occurred while processing the files. Please try again.");
  }
}

/**
 * Reads a File object and returns its Base64-encoded content.
 * @param {File} file - The File object to read.
 * @returns {Promise<string>} - A promise that resolves to the Base64-encoded content of the file.
 */
function readFileAsBase64(file) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();

    // Define the onload handler to resolve the promise with Base64 content
    reader.onload = () => {
      // reader.result is a data URL of the form "data:<mime>;base64,<data>"
      const base64 = reader.result.split(",")[1]; // Extract the Base64 part
      resolve(base64);
    };

    // Define the onerror handler to reject the promise on failure
    reader.onerror = () => {
      reader.abort(); // Abort the read operation
      reject(new Error("Failed to read file: " + file.name));
    };

    // Start reading the file as a Data URL
    reader.readAsDataURL(file);
  });
}

function updateStatus(taskId) {
  genericRequest("GetProcessingStatus", { taskId: taskId }, (data) => {
    const statusDiv = document.getElementById(
      "wildcardImporter-processing-status"
    );
    statusDiv.innerHTML = `
          <h4>Processing Status</h4>
          <p>Status: ${data.Status}</p>
          <p>Progress: ${(data.Progress)}%</p>
      `;

    if (data.conflicts && data.conflicts.length > 0) {
      statusDiv.innerHTML += `<h4>Conflicts</h4>`;
      data.Conflicts.forEach((conflict) => {
        statusDiv.innerHTML += `
                  <p>Conflict for file: ${conflict.filePath}</p>
                  <button onclick="wildcardImporterResolveConflict('${taskId}', '${conflict.filePath}', 'overwrite')">Overwrite</button>
                  <button onclick="wildcardImporterResolveConflict('${taskId}', '${conflict.filePath}', 'rename')">Rename</button>
                  <button onclick="wildcardImporterResolveConflict('${taskId}', '${conflict.filePath}', 'skip')">Skip</button>
              `;
      });
    }

    if (data.status !== "Completed") {
      setTimeout(() => updateStatus(taskId), 1000);
    } else {
      updateHistory();
    }
  });
}

function wildcardImporterResolveConflict(taskId, filePath, resolution) {
  genericRequest(
    "ResolveConflict",
    { taskId: taskId, filePath: filePath, resolution: resolution },
    (data) => {
      if (data.success) {
        alert("Conflict resolved");
        updateStatus(taskId);
      } else {
        alert("Error resolving conflict: " + data.message);
      }
    }
  );
}

function undoProcessing() {
  genericRequest("UndoProcessing", {}, (data) => {
    if (data.success) {
      alert("Processing undone successfully");
      updateHistory();
    } else {
      alert("Error undoing processing: " + data.message);
    }
  });
}

function updateHistory() {
  genericRequest("GetProcessingHistory", {}, (data) => {
    const historyDiv = document.getElementById(
      "wildcardImporter-processing-history"
    );
    historyDiv.innerHTML = "<h4>Processing History</h4>";
    if (data.history && data.history.length > 0) {
      const historyList = document.createElement("ul");
      data.history.forEach((item) => {
        const listItem = document.createElement("li");
        listItem.textContent = `${new Date(item.timestamp).toLocaleString()}: ${
          item.description
        }`;
        historyList.appendChild(listItem);
      });
      historyDiv.appendChild(historyList);
    } else {
      historyDiv.innerHTML += "<p>No processing history available.</p>";
    }
  });
}

function updateDestinationFolder() {
  // TODO: Allow setting the destination folder via API. Users might want to set it to their own custom folder.
  const destinationFolder = document.getElementById(
    "wildcardImporter-destination-folder"
  );
  genericRequest("GetDestinationFolder", {}, (data) => {
    destinationFolder.textContent = data.folderPath;
  });
}
