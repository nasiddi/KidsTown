import React, {Component} from 'react';
import Select from 'react-select';
import TextField from '@material-ui/core/TextField';
import FormControlLabel from '@material-ui/core/FormControlLabel';
import Checkbox from '@material-ui/core/Checkbox';
import 'bootstrap/dist/css/bootstrap.css';
import { Grid, MuiThemeProvider} from '@material-ui/core';
import {
    fetchLocations,
    getSelectedEventFromStorage,
    getSelectedOptionsFromStorage,
    getStateFromLocalStorage,
    selectStyles,
    theme
} from "./Common";
import {Alert, Button, ButtonGroup} from "reactstrap";
import {withAuth} from "../auth/MsalAuthProvider";

class CheckIn extends Component {
    static displayName = CheckIn.name;
    constructor(props) {
        super(props);
        this.securityCodeInput = React.createRef();

        this.state = {
            locations: [],
            checkInOutCandidates: [],
            selectedLocations: getSelectedOptionsFromStorage(
                'selectedLocations',
                []
            ),
            securityCode: '',
            fastCheckInOut: getStateFromLocalStorage('fastCheckInOut'),
            singleCheckInOut: getStateFromLocalStorage('singleCheckInOut'),
            alert: { text: '', level: 1 },
            checkType: localStorage.getItem('checkType') ?? 'CheckIn',
            loading: true,
        };
    }

    async componentDidMount() {
        const locations = await fetchLocations();
        this.setState({locations: locations, loading: false})
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
                                onClick={() => this.selectCheckType('CheckIn')}
                                color='primary'
                                outline={this.checkTypeIsActive('CheckIn')}
                            >CheckIn
                            </Button>
                            <Button
                                onClick={() => this.selectCheckType('CheckOut')}
                                color='primary'
                                outline={this.checkTypeIsActive('CheckOut')}
                            >CheckOut
                            </Button>
                        </ButtonGroup>
                    </Grid>
                    <MuiThemeProvider theme={theme}>
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
                        </MuiThemeProvider>
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
                        <MuiThemeProvider theme={theme}>
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
                        </MuiThemeProvider>
                    </Grid>
                    <Grid item md={2} xs={12}>
                        <Button
                            size='lg'
                            color={this.state.checkInOutCandidates.length > 0 ? 'secondary' : 'primary'}
                            block
                            onClick={this.state.checkInOutCandidates.length > 0 ? 
                                () => this.resetView() : () => this.submitSecurityCode()}
                        >{this.state.checkInOutCandidates.length > 0 ? 'Clear' : 'Search'}
                        </Button>
                    </Grid>
                </Grid>
            </div>
        );
    }

    renderAlert() {
        return (
            <div className='alertMessage'>
            <Alert color={this.state.alert.level.toLowerCase()}>
                {this.state.alert.text}
            </Alert>
            </div>
        );
    }

    renderSingleCheckout(checkInOutCandidates) {
        let candidates = checkInOutCandidates.map((c) => (
            <div className='nameButton' key={c['checkInId']}>
                <Button
                    block
                    color={this.getNameButtonColor(c)}
                    size="lg"
                    onClick={() => this.checkInOutSingle(c)}
                >{c['name']}
                </Button>
            </div>
        ));
        return <div>{candidates}</div>;
    }

    renderMultiCheckout(checkInOutCandidates) {
        let candidates = checkInOutCandidates.map((c) => (
            <div className='nameButton' key={c['checkInId']}>
                <Button
                    block
                    color={this.getNameButtonColor(c)}
                    size="lg"
                    outline={!c.isSelected}
                    onClick={() => this.invertSelectCandidate(c)}
                >{c['name']}
                </Button>
            </div>
        ));
        
        return (
            <div>
                {candidates}
                <div className='saveButton'>
                    <Button
                        block
                        color={this.areNoCandidatesSelected() ? 'secondary' : 'success'}
                        size="lg"
                        onClick={() => this.checkInOutMultiple()}
                        disabled={this.areNoCandidatesSelected()}
                    >{this.state.checkType}
                    </Button>
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
                eventId: await getSelectedEventFromStorage(),
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

    resetView(resetAlert = true) {
        this.focus();
        this.setState({ checkInOutCandidates: [], securityCode: ''});
        
        if (resetAlert){
            this.setState({ alert: { text: '', level: 1 }});
        }
    }

    invertSelectCandidate(candidate) {
        candidate.isSelected = !candidate.isSelected;
        this.setState({ checkInOutCandidates: this.state.checkInOutCandidates });
    }

    areNoCandidatesSelected() {
        return this.state.checkInOutCandidates.filter((c) => c.isSelected).length <= 0;
    }
    
    checkTypeIsActive(checkType) {
        return !(this.state.checkType === checkType);
    }

    selectCheckType(checkType) {
        if (this.state.checkType !== checkType){
            this.setState({ 'checkType': checkType})
            localStorage.setItem('checkType', checkType);
            this.resetView();
        }
    }

    getNameButtonColor(candidate) {
        if (this.state.checkType === 'CheckIn'){
            return 'primary';
        }
        
        if (candidate['hasPeopleWithoutPickupPermission']) {
            return 'danger';
        }

        if (!candidate['mayLeaveAlone']) {
            return 'warning';
        }

        return 'primary';
    }
}

export const CheckInOut = withAuth(CheckIn);
