'use client'
import {ThemeProvider} from "@mui/material";
import {paletteOverrideTheme} from "@/app/components/Common";
import Documentation from "@/app/documentation/page";

export default function Home() {
    return (
        <div>
            <ThemeProvider theme={paletteOverrideTheme}>
                <Documentation/>
            </ThemeProvider>
        </div>
    )
}
