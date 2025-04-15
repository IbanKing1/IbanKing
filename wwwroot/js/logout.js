let inactivityTime = function () {
    let time;
    const redirectUrl = '/Login';

    function resetTimer() {
        clearTimeout(time);
        time = setTimeout(() => {
            window.location.href = redirectUrl;
        }, 10 * 1000); 
    }

    window.onload = resetTimer;
    document.onmousemove = resetTimer;
    document.onkeydown = resetTimer;
    document.onclick = resetTimer;
    document.onscroll = resetTimer;
};

inactivityTime();