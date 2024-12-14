(() => {
    'use strict'

    // CONSTANTS
    const HTML_ELEMENT = document.documentElement
    const THEME = "appTheme"
    const THEME_ATTRIBUTE = "data-bs-theme"
    const DARK = "dark"
    const LIGHT = "light"


    // FUNCTIONS
    // Function that returns an array of available themes, with the first element representing the DARK theme and the second element representing the LIGHT theme
    window.getAvailableAppThemes = () => [DARK, LIGHT]

    // Function that gets the stored App Theme from the localStorage
    window.getStoredAppTheme = () => localStorage.getItem(THEME)

    // Function that gets the browser's default App Theme
    window.getBrowsersAppTheme = () => window.matchMedia(`(prefers-color-scheme: ${DARK})`).matches ? DARK : LIGHT

    // Function that gets the currently set App Theme
    window.getCurrentAppTheme = () => HTML_ELEMENT.getAttribute(THEME_ATTRIBUTE)

    // Function that gets the preferred App Theme
    window.getPreferredAppTheme = () => {
        const storedTheme = getStoredAppTheme()
        switch (storedTheme) {
            case LIGHT:
            case DARK:
                return storedTheme
            default:
                return getBrowsersAppTheme()
        }
    }

    // Function that sets the App Theme
    window.setAppTheme = theme => {
        HTML_ELEMENT.setAttribute(THEME_ATTRIBUTE, theme)
        localStorage.setItem(THEME, theme)
    }

    // Function that sets Dark App Theme
    window.setAppThemeDark = () => setAppTheme(DARK)

    // Function that sets Light App Theme
    window.setAppThemeLight = () => setAppTheme(LIGHT)
    
    // Function that toggles the currently set App Theme
    window.toggleAppTheme = () => setAppTheme(getCurrentAppTheme() === DARK ? LIGHT : DARK)


    // MAIN (sets theme on initialization)
    setAppTheme(getPreferredAppTheme())
})()