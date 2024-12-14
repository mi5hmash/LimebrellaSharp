(() => {
    'use strict'

    // Function to clear the file input
    window.resetFileInput = (inputElement) => {
        if (inputElement) {
            inputElement.value = ""; 
        }
    }

    // Function to save a file locally from a byte array in the browser
    window.saveAsFile = (fileName, byteArray) => {
        var blob = new Blob([new Uint8Array(byteArray)], { type: "application/octet-stream" });
        var link = document.createElement('a');
        link.href = URL.createObjectURL(blob);
        link.download = fileName;
        link.click();
        URL.revokeObjectURL(link.href);
    }
})()