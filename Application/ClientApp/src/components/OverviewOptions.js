import React, { Component } from 'react';
import Select from "react-select";
import {Grid} from "@material-ui/core";
import {
    fetchLocations,
    getStringFromSession,
    getSelectedOptionsFromStorage,
    selectStyles,
    getLastSunday
} from "./Common";
import DatePicker from "reactstrap-date-picker";
import {withAuth} from "../auth/MsalAuthProvider";

class Options extends Component {
    static displayName = Options.name;
    repeat;


    constructor (props) {
        super(props);
        
        this.state = {
            locations: [],
            overviewLocations: getSelectedOptionsFromStorage(
                'overviewLocations',
                []
            ),
            loading: true,
            date: ''
        };
    }

    async componentDidMount() {
        const locations = await fetchLocations();
        this.setState({date: getStringFromSession('overviewDate', getLastSunday().toISOString())})
        this.setState({locations: locations})
        this.setState({loading: false})
    }
    
    renderOptions(){
        return <div>
            <Grid container spacing={3} justify="space-between" alignItems="center">
                <Grid item xs={9}>
                    <Select
                        styles={selectStyles}
                        isMulti
                        placeholder='Select locations'
                        name='overviewLocations'
                        options={this.state.locations}
                        className='basic-multi-select'
                        classNamePrefix='select'
                        onChange={(o) => this.updateOptions(o, 'overviewLocations')}
                        defaultValue={this.state.overviewLocations}
                        minWidth='100px'
                    />
                </Grid>
            <Grid item xs={3}>
                <DatePicker 
                    id="datepicker"
                    value={this.state.date}
                    dateFormat='DD.MM.YYYY'
                    onClear={() => {this.updateDate(getLastSunday().toISOString())}}
                    showClearButton
                    onChange= {(v) => this.updateDate(v)} />
                </Grid>
            </Grid>
        </div>
    }
    render () {
        if (this.state.loading){
            return (
                <div/>
            );
        }

        return (
            <div>
                <Grid container spacing={3} justify="space-between" alignItems="flex-start">
                    <Grid item xs={12}>
                        {this.renderOptions()}
                    </Grid>
                </Grid>
            </div>
        )
    }

    updateOptions = (options, key) => {
        localStorage.setItem(key, JSON.stringify(options));
        this.setState({ [key]: options });
    };


    updateDate(v) {
        if (v === null){
            return;
        }
        
        sessionStorage.setItem('overviewDate', v)
        this.setState({date: v})
    }
}

export const OverviewOptions = withAuth(Options);

