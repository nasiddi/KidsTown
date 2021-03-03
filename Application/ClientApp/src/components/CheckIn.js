import React, {Component} from 'react';
import Select from 'react-select';
import TextField from '@material-ui/core/TextField';
import FormControlLabel from '@material-ui/core/FormControlLabel';
import Checkbox from '@material-ui/core/Checkbox';
import Alert from '@material-ui/lab/Alert';
import 'bootstrap/dist/css/bootstrap.css';
import {Button, ButtonGroup, Grid} from '@material-ui/core';
import { withStyles } from '@material-ui/core/styles';

import {green, red, indigo, amber} from '@material-ui/core/colors';

const selectStyles = {
    menu: (base) => ({
        ...base,
        zIndex: 100,
    }),
};

const styledBy = (property, mapping) => (props) => mapping[props[property]];

const styles = {
    root: {
        background: styledBy('backgroundColor', {
            blue: indigo[500],
            red: red[500],
            yellow: amber[500],
            green: green[500],
            white: 'white'
        }),
        "&:hover": {
            backgroundColor: styledBy( 'backgroundColor', {
                blue: indigo[700],
                red: red[700],
                yellow: amber[700],
                green: green[700],
                white: 'lightgray'
            })
        },
        '&:disabled': {
            backgroundColor: 'lightgray',
            borderColor: 'lightgrey'
        },
        color: styledBy('fontColor', {
            blue: indigo[500],
            red: red[500],
            yellow: amber[500],
            green: green[500],
            white: 'white'
        }),
        border: '2px solid',
        borderColor: styledBy('borderColor', {
            blue: indigo[500],
            red: red[500],
            yellow: amber[500],
            green: green[500],
            white: 'white'
        })
    },
};

const StyledButton = withStyles(styles)(({ classes, backgroundColor, fontColor, borderColor, ...other }) => (
    <Button className={classes.root} {...other} />
));


export class CheckIn extends Component {
    static displayName = CheckIn.name;
    constructor(props) {
        super(props);
        this.securityCodeInput = React.createRef();

        this.state = {
            locations: [],
            checkInOutCandidates: [],
            selectedLocations: this.getSelectedOptionsFromStorage(
                'selectedLocations',
                []
            ),
            securityCode: '',
            fastCheckInOut: this.getStateFromLocalStorage('fastCheckInOut'),
            singleCheckInOut: this.getStateFromLocalStorage('singleCheckInOut'),
            alert: { text: '', level: 1 },
            checkType: localStorage.getItem('checkType') ?? 'CheckIn',
            loading: true,
        };
    }

    componentDidMount() {
        this.fetchData().then();
    }

    componentDidUpdate(prevProps, prevState, snapshot) {
        if (
            prevState.securityCode.length !== 4 &&
            this.state.securityCode.length === 4
        ) {
            this.submitSecurityCode().then();
        }
    }

    focus() {
        this.securityCodeInput.current.focus();
    }

    renderOptionsAndInput() {
        return (
            <div className='option-div'>
                <Grid container spacing={3} justify="space-between" alignItems="center">
                    <Grid item md={3} xs={12}>
                        <ButtonGroup size="medium" color="primary">
                            <Button
                                variant={this.checkTypeIsActive('CheckIn')}
                                onClick={() => this.selectCheckType('CheckIn')}
                                disableElevation
                            >CheckIn
                            </Button>
                            <Button
                                variant={this.checkTypeIsActive('CheckOut')}
                                onClick={() => this.selectCheckType('CheckOut')}
                                disableElevation
                            >CheckOut
                            </Button>
                        </ButtonGroup>
                    </Grid>
                    <Grid item md={3} xs={6}>
                        <FormControlLabel
                            control={
                                <Checkbox
                                    name='fastCheckInOut'
                                    color='primary'
                                    checked={this.state.fastCheckInOut}
                                    onChange={this.handleChange}
                                />
                            }
                            label={`Fast ${this.state.checkType}`}
                            labelPlacement='end'
                        />
                    </Grid>
                    <Grid item md={3} xs={6}>
                        <FormControlLabel
                            control={
                                <Checkbox
                                    name='singleCheckInOut'
                                    color='primary'
                                    checked={this.state.singleCheckInOut}
                                    onChange={this.handleChange}
                                />
                            }
                            label={`Single ${this.state.checkType}`}
                            labelPlacement='end'
                        />
                    </Grid>
                    <Grid item md={3} xs={12}>
                        <Select
                            styles={selectStyles}
                            isMulti
                            placeholder='Select locations'
                            name='locations'
                            options={this.state.locations}
                            className='basic-multi-select'
                            classNamePrefix='select'
                            onChange={(o) => this.updateOptions(o, 'selectedLocations')}
                            defaultValue={this.state.selectedLocations}
                            minWidth='100px'
                        />
                    </Grid>
                    <Grid item md={10} xs={12}>
                        <TextField
                            inputRef={this.securityCodeInput}
                            id='outlined-basic'
                            label='SecurityCode'
                            variant='outlined'
                            value={this.state.securityCode}
                            onChange={this.updateSecurityCode}
                            fullWidth={true}
                            autoFocus
                        />
                    </Grid>
                    <Grid item md={2} xs={12}>
                        <Button
                            size='large'
                            variant='contained'
                            color={this.state.checkInOutCandidates.length > 0 ? 'default' : 'primary'}
                            fullWidth={true}
                            disableElevation
                            onClick={this.state.checkInOutCandidates.length > 0 ? 
                                () => this.resetView() : () => this.submitSecurityCode()}
                        >
                            {this.state.checkInOutCandidates.length > 0 ? 'Clear' : 'Search'}
                        </Button>
                    </Grid>
                </Grid>
            </div>
        );
    }

    renderAlert() {
        return (
            <div className='alertMessage'>
            <Alert severity={this.state.alert.level.toLowerCase()}>
                {this.state.alert.text}
            </Alert>
            </div>
        );
    }

    renderSingleCheckout(checkInOutCandidates) {
        let candidates = checkInOutCandidates.map((c) => (
            <div className='nameButton' key={c['checkInId']}>
                <StyledButton
                    backgroundColor={this.getNameButtonBackgroundColor(c)}
                    fontColor={this.getNameButtonFontColor(c)}
                    borderColor={this.getNameButtonBorderColor(c)}                    
                    onClick={() => this.checkInOutSingle(c)}
                    size='large'
                    fullWidth={true}
                >
                    {c['name']}
                </StyledButton>
            </div>
        ));
        return <div>{candidates}</div>;
    }

    renderMultiCheckout(checkInOutCandidates) {
        let candidates = checkInOutCandidates.map((c) => (
            <div className='nameButton' key={c['checkInId']}>
                <StyledButton
                    backgroundColor={this.getNameButtonBackgroundColor(c)}
                    fontColor={this.getNameButtonFontColor(c)}
                    borderColor={this.getNameButtonBorderColor(c)}
                    onClick={() => this.invertSelectCandidate(c)}
                    size='large'
                    fullWidth={true}
                >
                    {c['name']}
                </StyledButton>
            </div>
        ));
        
        return (
            <div>
                {candidates}
                <div className='saveButton'>
                    <StyledButton
                        disabled={this.areAnyCandidatesSelected()}
                        backgroundColor='green'
                        fontColor='white'
                        borderColor={'green'}
                        onClick={() => this.checkInOutMultiple()}
                        size='large'
                        fullWidth={true}
                    >
                        {this.state.checkType}
                    </StyledButton>
                </div>
            </div>
        );
    }

    render() {
        let options = this.state.loading ? (
            <p>
                <em>Loading...</em>
            </p>
        ) : (
            this.renderOptionsAndInput()
            );
        
        let alert = this.state.alert.text.length > 0 ? this.renderAlert() : <div />;

        let candidates = <div />;
        if (this.state.singleCheckInOut) {
            candidates = this.renderSingleCheckout(this.state.checkInOutCandidates);
        } else if (this.state.checkInOutCandidates.length > 0) {
            candidates = this.renderMultiCheckout(this.state.checkInOutCandidates);
        }

        return (
            <div>
                <h1 id='title'>{this.state.checkType}</h1>
                {options}
                {alert}
                {candidates}
            </div>
        );
    }

    handleChange = (event) => {
        localStorage.setItem(event.target.name, event.target.checked);
        this.setState({ [event.target.name]: event.target.checked });
        this.focus();
    };

    updateOptions = (options, key) => {
        localStorage.setItem(key, JSON.stringify(options));
        this.setState({ [key]: options });
        this.resetView();
    };

    updateSecurityCode = (e) => {
        this.setState({ securityCode: e.target.value });
    };

    async submitSecurityCode() {
        let selectedLocationIds = this.state.selectedLocations.map((l) => l.value);
        await fetch('checkinout/people', {
            body: JSON.stringify({
                securityCode: this.state.securityCode,
                selectedLocationIds: selectedLocationIds,
                isFastCheckInOut: this.state.fastCheckInOut ?? false,
                checkType: this.state.checkType,
                checkInIds: [],
            }),
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
        })
            .then((r) => r.json())
            .then((j) => {
                this.setState({ alert: { level: j['alertLevel'], text: j['text'] } });
                if (j['successfulFastCheckout'] === true) {
                    this.resetView(false);
                } else {
                    let candidates = j['checkInOutCandidates'].map(function (el) {
                        let o = Object.assign({}, el);
                        o.isSelected = true;
                        return o;
                    });

                    this.setState({ checkInOutCandidates: candidates });
                }
            });
    }

    async checkInOutSingle(checkInOutCandidate) {
        await fetch('checkinout/manual', {
            body: JSON.stringify({
                checkType: this.state.checkType,
                checkInOutCandidates: [checkInOutCandidate],
            }),
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
        })
            .then((r) => r.json())
            .then((j) => {
                this.setState({ alert: { level: j['alertLevel'], text: j['text'] } });
                this.resetView(false);
            });
    }

    async checkInOutMultiple() {
        let candidates = this.state.checkInOutCandidates.filter((c) => c.isSelected);
        await fetch('checkinout/manual', {
            body: JSON.stringify({
                checkType: this.state.checkType,
                checkInOutCandidates: candidates,
            }),
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
        })
            .then((r) => r.json())
            .then((j) => {
                this.setState({ alert: { level: j['alertLevel'], text: j['text'] } });
                this.resetView(false);
            });
    }

    async fetchData() {
        const response = await fetch('checkinout/location');
        const data = await response.json();
        this.setState({ locations: data, loading: false });
    }

    resetView(resetAlert = true) {
        this.focus();
        this.setState({ checkInOutCandidates: [], securityCode: ''});
        
        if (resetAlert){
            this.setState({ alert: { text: '', level: 1 }});
        }
    }

    getStateFromLocalStorage(boolean) {
        let s = localStorage.getItem(boolean);
        return s === undefined ? false : JSON.parse(s);
    }

    getSelectedOptionsFromStorage(key, fallback) {
        let s = localStorage.getItem(key);
        return s === null ? fallback : JSON.parse(s);
    }

    invertSelectCandidate(candidate) {
        candidate.isSelected = !candidate.isSelected;
        this.setState({ checkInOutCandidates: this.state.checkInOutCandidates });
    }

    areAnyCandidatesSelected() {
        return this.state.checkInOutCandidates.filter((c) => c.isSelected).length <= 0;
    }
    
    getNameButtonBackgroundColor(candidate) {
        if (!candidate.isSelected) {
            return 'white'
        }

        if (candidate['hasPeopleWithoutPickupPermission']) {
            return 'red';
        }

        if (!candidate['mayLeaveAlone']) {
            return 'yellow';
        }

        return 'blue';
        
    }

    getNameButtonFontColor(candidate) {
        if (candidate.isSelected) {
            return 'white'
        }

        if (candidate['hasPeopleWithoutPickupPermission']) {
            return 'red';
        }

        if (!candidate['mayLeaveAlone']) {
            return 'yellow';
        }

        return 'blue';
    }

    getNameButtonBorderColor(candidate) {
        if (candidate['hasPeopleWithoutPickupPermission']) {
            return 'red';
        }

        if (!candidate['mayLeaveAlone']) {
            return 'yellow';
        }

        return 'blue';
    }

    checkTypeIsActive(checkType) {
        if (this.state.checkType === checkType){
            return 'contained';
        }
        
        return 'outlined';
    }

    selectCheckType(checkType) {
        if (this.state.checkType !== checkType){
            this.setState({ 'checkType': checkType})
            localStorage.setItem('checkType', checkType);
            this.resetView();
        }
    }
}
