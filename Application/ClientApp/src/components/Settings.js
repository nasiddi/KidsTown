import React, { Component } from 'react';
import {Grid} from "@material-ui/core";
import Checkbox from "@material-ui/core/Checkbox";
import FormControlLabel from "@material-ui/core/FormControlLabel";
import {getSelectedEventFromStorage} from "./Common";
import {withAuth} from "../auth/MsalAuthProvider";

class Setting extends Component {
    static displayName = Setting.name;
    
    repeat 
    
    constructor (props) {
        super(props);

        this.state = {
            events: [],
            selectedEvent: 0,
            loading: true
        };
    }

    async componentDidMount() {
        await this.fetchAvailableEvents()
        this.setState({ loading: false})
    }

    componentWillUnmount() {
        clearTimeout(this.repeat);
    }
    
    renderOptions(){

        let events = this.state.events.map((e) => (
            <Grid item xs={12} key={e['eventId']}>
            <div className='event'>
                <FormControlLabel
                    control={
                        <Checkbox
                            name={e['name']}
                            color='primary'
                            onChange={this.handleChange}
                            checked={this.state.selectedEvent === e['eventId']}
                        />
                    }
                    label={''}
                    labelPlacement='end'
                />
                <a 
                    href={`https://check-ins.planningcenteronline.com/events/${e['eventId']}`}
                    target="_blank"
                    rel="noopener noreferrer"
                >{e['name']}</a>
            </div>
            </Grid>
        ));
        
        return <div>
            <h2>Event</h2>
            <Grid container spacing={1} justify="space-between" alignItems="center">
                {events}  
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
                        <h1>Settings</h1>
                    </Grid>
                    <Grid item xs={6}>
                        {this.renderOptions()}
                    </Grid>
                </Grid>
            </div>
        )
    }

    updateOptions = async (options, key) => {
        localStorage.setItem(key, JSON.stringify(options));
        this.setState({[key]: options});
    };

    async fetchAvailableEvents() {
        const response = await fetch('configuration/events');
        const events = await response.json();
        let selected = await getSelectedEventFromStorage();
        this.setState({events: events, selectedEvent: selected})
    }

    handleChange = (event) => {
        if (event.target.checked){
            
            let selected = this.state.events.find(e => e['name'] === event.target.name)
            localStorage.setItem('selectedEvent', selected['eventId']);
            this.setState({selectedEvent: selected['eventId']})
        }
    }
}

export const Settings = withAuth(Setting);
