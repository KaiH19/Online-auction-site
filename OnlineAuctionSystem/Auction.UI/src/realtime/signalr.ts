import * as signalR from '@microsoft/signalr';

const API_BASE = import.meta.env.VITE_API_BASE ?? 'http://localhost:5030';
let connection: signalR.HubConnection | null = null;

export function getToken(): string | null {
  // Adjust to your auth storage (localStorage/session/cookie)
  return localStorage.getItem('jwt'); 
}

export async function getConnection(): Promise<signalR.HubConnection> {
  if (connection && connection.state === signalR.HubConnectionState.Connected) {
    return connection;
  }

  connection = new signalR.HubConnectionBuilder()
    .withUrl(`${API_BASE}/hubs/bidding`, {
      accessTokenFactory: () => getToken() ?? "",
      transport: signalR.HttpTransportType.WebSockets,
      skipNegotiation: true
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000])
    .build();

  await connection.start();
  return connection;
}

export async function joinAuctionRoom(auctionId: number): Promise<void> {
  const conn = await getConnection();
  await conn.invoke('JoinAuction', auctionId);
}

export async function leaveAuctionRoom(auctionId: number): Promise<void> {
  const conn = await getConnection();
  await conn.invoke('LeaveAuction', auctionId);
}

export function onBidPlaced(handler: (payload: any) => void) {
  getConnection().then(conn => conn.on('BidPlaced', handler));
}

export function onAuctionClosed(handler: (payload: any) => void) {
  getConnection().then(conn => conn.on('AuctionClosed', handler));
}
