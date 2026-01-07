// JavaScript interop functions for Storingsdienst

/**
 * Downloads a file from a base64-encoded stream
 * @param {string} fileName - The name of the file to download
 * @param {string} base64Data - The file data encoded as base64
 */
window.downloadFileFromStream = async (fileName, base64Data) => {
    // Convert base64 to byte array
    const byteCharacters = atob(base64Data);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);

    // Create blob and download link
    const blob = new Blob([byteArray], {
        type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
    });

    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;

    // Trigger download
    document.body.appendChild(link);
    link.click();

    // Cleanup
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
};
