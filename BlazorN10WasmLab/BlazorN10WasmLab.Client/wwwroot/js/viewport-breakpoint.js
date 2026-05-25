let mql = null;
let dotNetRef = null;
let handler = null;

export function init(mediaQuery, ref) {
    mql = window.matchMedia(mediaQuery);
    dotNetRef = ref;
    handler = (e) => dotNetRef.invokeMethodAsync('OnMatchChanged', e.matches);
    mql.addEventListener('change', handler);
    return mql.matches;
}

export function dispose() {
    if (mql && handler) mql.removeEventListener('change', handler);
    mql = null;
    handler = null;
    dotNetRef = null;
}
