export function SubscribeForChange(element, objRef) {
    window.addEventListener('resize', () => {
        objRef.invokeMethod('InvokeCallback', element.getBoundingClientRect());
    });

    objRef.invokeMethod('InvokeCallback', element.getBoundingClientRect());
}

export function UnSubscribeForChange() {

}