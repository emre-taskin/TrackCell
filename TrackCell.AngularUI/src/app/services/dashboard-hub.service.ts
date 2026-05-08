import { Injectable } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState
} from '@microsoft/signalr';
import { Observable, Subject } from 'rxjs';
import { environment } from '../../environments/environment';

export type ConnectionStatus = 'connecting' | 'connected' | 'reconnecting' | 'disconnected';

@Injectable({ providedIn: 'root' })
export class DashboardHubService {
  private connection?: HubConnection;
  private updates$ = new Subject<void>();
  private status$ = new Subject<ConnectionStatus>();

  get onUpdate(): Observable<void> {
    return this.updates$.asObservable();
  }

  get onStatus(): Observable<ConnectionStatus> {
    return this.status$.asObservable();
  }

  async start(): Promise<void> {
    if (this.connection && this.connection.state !== HubConnectionState.Disconnected) {
      return;
    }
    this.connection = new HubConnectionBuilder()
      .withUrl(environment.apiBase + environment.hubPath)
      .withAutomaticReconnect()
      .build();

    this.connection.on('UpdateDashboard', () => this.updates$.next());
    this.connection.onreconnecting(() => this.status$.next('reconnecting'));
    this.connection.onreconnected(() => this.status$.next('connected'));
    this.connection.onclose(() => this.status$.next('disconnected'));

    this.status$.next('connecting');
    try {
      await this.connection.start();
      this.status$.next('connected');
    } catch (err) {
      this.status$.next('disconnected');
      console.warn('SignalR connection failed; retrying in 5s', err);
      setTimeout(() => this.start(), 5000);
    }
  }

  async stop(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
    }
  }
}
