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

/**
 * Initialize progress bar tracking for the Power Automate guide
 * Tracks checkbox completion and updates the progress bar accordingly
 */
window.initializeProgressTracking = () => {
    // Total number of steps
    const totalSteps = 8;
    
    // Get all completion checkboxes
    const checkboxes = document.querySelectorAll('.completion-checkbox');
    
    // Function to update progress bar
    const updateProgress = () => {
        const completedSteps = document.querySelectorAll('.completion-checkbox:checked').length;
        const percentage = (completedSteps / totalSteps) * 100;
        
        const progressBar = document.querySelector('.guide-progress .progress-bar');
        const progressText = document.querySelector('#progress-text');
        
        if (progressBar && progressText) {
            progressBar.style.width = `${percentage}%`;
            progressBar.setAttribute('aria-valuenow', completedSteps);
            progressText.textContent = `${completedSteps} of ${totalSteps} steps completed`;
        }
        
        // Store progress in localStorage
        const completedIds = Array.from(document.querySelectorAll('.completion-checkbox:checked'))
            .map(cb => cb.id);
        localStorage.setItem('guideProgress', JSON.stringify(completedIds));
    };
    
    // Restore progress from localStorage
    const restoreProgress = () => {
        const savedProgress = localStorage.getItem('guideProgress');
        if (savedProgress) {
            try {
                const completedIds = JSON.parse(savedProgress);
                completedIds.forEach(id => {
                    const checkbox = document.getElementById(id);
                    if (checkbox) {
                        checkbox.checked = true;
                    }
                });
                updateProgress();
            } catch (e) {
                console.error('Failed to restore progress:', e);
            }
        }
    };
    
    // Add event listeners to all checkboxes
    checkboxes.forEach(checkbox => {
        checkbox.addEventListener('change', updateProgress);
    });
    
    // Restore progress on load
    restoreProgress();
};
