import React from 'react';
import { loadCSS } from 'fg-loadcss';
import Icon from '@material-ui/core/Icon';
import { Grid } from "@material-ui/core";
import {Image} from "react-bootstrap";
import PropTypes from 'prop-types';

const action = 'far fa-arrow-alt-circle-right';
const info = 'fas fa-info-circle';
const warning = 'fas fa-exclamation-triangle';

export function DocsIcon(props) {
    React.useEffect(() => {
        const node = loadCSS(
            'https://use.fontawesome.com/releases/v5.12.0/css/all.css',
            document.querySelector('#font-awesome-css'),
        );

        return () => {
            node.parentNode.removeChild(node);
        };
    }, []);

    return (<Icon className={props['name']}/>);
}

export function Title(props) {
    const TitleSize = `h${props['size']}`;

    return (<Grid item xs={12}><TitleSize>{props['text']}</TitleSize></Grid>
    );
}

Title.propTypes = {
    text: PropTypes.string.isRequired,
    size: PropTypes.number.isRequired
}

export function SplitImage(props) {
    const [open, setOpen] = React.useState(6);

    let image = '';
    try {
        image = require(`../docs/${props['fileName']}`);
    }
    catch (e) {}

    function handleOpen() {
        if (open === 6){
            setOpen(12);
        } 
        else {
            setOpen(6);
        }
    }

    let mediaCard = <Image src={image} alt={props['fileName']} onClick={handleOpen} fluid/>
    return <Grid item sm={open} xs={12}>{mediaCard}</Grid>;
}

export function SplitImageInner(props, func, open) {

    let image = '';
    try {
        image = require(`../docs/${props['fileName']}`);
    }
    catch (e) {}

    let mediaCard = <Image src={image} alt={props['fileName']} onClick={func} fluid/>
    return <Grid item sm={open} xs={12}>{mediaCard}</Grid>;
}

SplitImage.propTypes = {
    fileName: PropTypes.string.isRequired
}

export function SplitText(props) {
    return stylizedText(props, 6)
}

export function FullText(props) {
    return stylizedText(props, 12)
}

FullText.propTypes = {
    title: PropTypes.string,
    paragraphs: PropTypes.arrayOf(PropTypes.shape({
        icon: PropTypes.oneOf(['Info', 'Action']),
        text: PropTypes.string.isRequired
    })).isRequired
}


export function TextImageSplit(props){
    const [open, setOpen] = React.useState(6);

    let image = '';
    try {
        image = require(`../docs/${props['fileName']}`);
    }
    catch (e) {}

    function handleOpen() {
        if (open === 6){
            setOpen(12);
        }
        else {
            setOpen(6);
        }
    }
    
    return (
        <Grid item xs={12}>
        <Grid container spacing={3} justify="space-between" alignItems="flex-start">
            {stylizedText(props, open)}
            {SplitImageInner(props, handleOpen, open)}
        </Grid>
        </Grid>
    )
}

TextImageSplit.propTypes = {
    title: PropTypes.string,
    fileName: PropTypes.string.isRequired,
    paragraphs: PropTypes.arrayOf(PropTypes.shape({
        icon: PropTypes.oneOf(['Info', 'Action']),
        text: PropTypes.string.isRequired
    })).isRequired
}
 
function stylizedText(props, size) {

    let paragraphs = props.paragraphs.map((p, index) => {
        let icon = <div/>;
        if (p.icon === 'Info') {
            icon = <div className="wrap-icon"><DocsIcon name={info}/></div>
        }
        if (p.icon === 'Action') {
            icon = <div className="wrap-icon"><DocsIcon name={action}/></div>
        }
        
        if (p.icon === 'Warning') {
            icon = <div className="wrap-icon"><DocsIcon name={warning}/></div>
        }

        return (<div key={index}>
            {icon}
            <p>
                {p['text']}
            </p>
        </div>)
    });

    return (<Grid item sm={size} xs={12}><h5>{props.title}</h5>{paragraphs}</Grid>);
}

SplitText.propTypes = {
    title: PropTypes.string,
    paragraphs: PropTypes.arrayOf(PropTypes.shape({
        icon: PropTypes.oneOf(['Info', 'Action', 'Warning']),
        text: PropTypes.string.isRequired
    })).isRequired
}
