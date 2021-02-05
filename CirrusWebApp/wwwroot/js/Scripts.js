window.UpdateBoxes = (ClassName) => {
    var boxes = document.getElementsByClassName(ClassName);
    for (let box of boxes)
        if (box.checked) {
            box.checked = false;
        }
        else {
            box.checked = true;
        }
}