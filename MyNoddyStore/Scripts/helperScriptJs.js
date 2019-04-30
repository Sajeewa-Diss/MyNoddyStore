function GetCountdownSecondsString(countdownDate) {
    // Get todays date and time
    var now = new Date().getTime();
    var formattedSeconds
    // Find the duration between now and the count down date
    var duration = countdownDate - now;

    // If the count down is finished, set to zero
    if (duration < 0) {
        formattedSeconds = "00"; 
    } else {
        // Time calculations for days, hours, minutes and seconds
        //var days = Math.floor(duration / (1000 * 60 * 60 * 24));
        //var hours = Math.floor((duration % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
        //var minutes = Math.floor((duration % (1000 * 60 * 60)) / (1000 * 60));
        var seconds = Math.floor((duration % (1000 * 60)) / 1000);
        formattedSeconds = seconds.toString().padStart(2, '0');
    }

    return formattedSeconds;
}

