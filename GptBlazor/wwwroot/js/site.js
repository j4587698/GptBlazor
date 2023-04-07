function scrollToBottom() {
    var element = document.getElementById("scroll");
    element.scrollTop = element.scrollHeight;
}

function highlight(){
        document.querySelectorAll('pre code').forEach((el) => {
            hljs.highlightElement(el);
    });
}
