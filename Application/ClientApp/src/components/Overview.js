import React, { Component } from 'react';
import Select from "react-select";
import {Grid} from "@material-ui/core";
import {fetchLocations, getSelectedOptionsFromStorage, selectStyles} from "./Common";
import { Table } from 'reactstrap';

export class Overview extends Component {
    static displayName = Overview.name;
    
    repeat 
    
    constructor (props) {
        super(props);

        this.state = {
            locations: [],
            overviewLocations: getSelectedOptionsFromStorage(
                'overviewLocations',
                []
            ),
            attendees: {},
            loading: true
        };
    }

    async componentDidMount() {
        const locations = await fetchLocations();
        this.setState({locations: locations})
        await this.fetchData();
        this.setState({ loading: false})
    }

    componentWillUnmount() {
        clearTimeout(this.repeat);
    }
    
    renderOptions(){
        return <div>
            <Grid container spacing={3} justify="space-between" alignItems="center">
                <Grid item xs={12}>
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
            </Grid>
        </div>
    }
    
    renderCounts(){
        if (this.state.loading){
            return (
                <div/>
            );
        }
        
        let volunteers = this.state.attendees.filter(a => a['attendanceType'] === 'Volunteer');
        let kids = this.state.attendees.filter(a => a['attendanceType'] === 'Regular' || a['attendanceType'] === 'Guest');
        
        let totalCount = <tr/>;
        if (this.state.overviewLocations.length !== 1){
            totalCount = 
                <tr key='Total'>
                <th>Total</th>
                <th align="right">{kids.length}</th>
                <th align="right">{volunteers.length}</th>
            </tr>
        }
        
        
        return (
            <Table>
                <thead>
                <tr>
                    <th>Location</th>
                    <th>Kids</th>
                    <th>Volunteers</th>
                </tr>
                </thead>
                <tbody>
                {this.state.overviewLocations.map((row) => (
                    <tr key={row.value}>
                        <td> {row.label}</td>
                        <td>{kids.filter(k => k['locationId'] === row.value).length}</td>
                        <td>{volunteers.filter(k => k['locationId'] === row.value).length}</td>
                    </tr>
                ))}
                {totalCount}
                </tbody>
            </Table>
        );
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
                    <Grid item md={6} xs={12}>
                        {this.renderCounts()}
                    </Grid>
                    <Grid item md={6} xs={12}>
                        {this.renderOptions()}
                    </Grid>
                </Grid>
            </div>
        )
    }

    async fetchData() {
        await fetch('checkinout/attendees/active', {
            body: JSON.stringify(this.state.overviewLocations.map(l => l.value)),
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
        })
            .then((r) => r.json())
            .then((j) => {
                this.setState({ attendees: j });
            });
        
        this.repeat = setTimeout(this.fetchData.bind(this), 500);
    }

    updateOptions = (options, key) => {
        localStorage.setItem(key, JSON.stringify(options));
        this.setState({ [key]: options });
    };
}