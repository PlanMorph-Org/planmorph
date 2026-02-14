export interface Design {
  id: string;
  title: string;
  description: string;
  price: number;
  bedrooms: number;
  bathrooms: number;
  squareFootage: number;
  stories: number;
  category: string;
  estimatedConstructionCost: number;
  previewImages: string[];
  previewVideos: string[];
}

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
}

export interface Order {
  id: string;
  orderNumber: string;
  designId: string;
  designTitle: string;
  amount: number;
  status: string;
  paymentMethod: string;
  createdAt: string;
  paidAt?: string;
  includesConstruction: boolean;
  constructionContract?: ConstructionContract;
}

export interface ConstructionContract {
  id: string;
  location: string;
  estimatedCost: number;
  commissionAmount: number;
  status: string;
  contractorName?: string;
}

export interface OrderFile {
  id: string;
  fileName: string;
  fileType: string;
  category: string;
  fileSizeBytes: number;
}

export interface CreateOrderDto {
  designId: string;
  paymentMethod: 'Paystack';
  includesConstruction: boolean;
  constructionLocation?: string;
  constructionCountry?: string;
}

export interface DesignFilter {
  minBedrooms?: number;
  maxBedrooms?: number;
  minBathrooms?: number;
  maxBathrooms?: number;
  minPrice?: number;
  maxPrice?: number;
  category?: string;
  stories?: number;
  minSquareFootage?: number;
  maxSquareFootage?: number;
}

// Support Ticket Types
export type TicketStatus = 'Open' | 'Assigned' | 'InProgress' | 'Resolved' | 'Closed';
export type TicketPriority = 'Low' | 'Medium' | 'High' | 'Urgent';
export type TicketCategory = 'Technical' | 'Billing' | 'Design' | 'Order' | 'Construction' | 'General';

export interface Ticket {
  id: string;
  ticketNumber: string;
  clientId: string;
  subject: string;
  description: string;
  status: TicketStatus;
  priority: TicketPriority;
  category: TicketCategory;
  assignedToAdminId?: string;
  orderId?: string;
  designId?: string;
  createdAt: string;
  updatedAt: string;
  closedAt?: string;
  messages: TicketMessage[];
  unreadMessageCount: number;
}

export interface TicketMessage {
  id: string;
  ticketId: string;
  authorId: string;
  authorName: string;
  content: string;
  isFromAdmin: boolean;
  isReadByClient: boolean;
  createdAt: string;
}

export interface CreateTicketDto {
  subject: string;
  description: string;
  category: TicketCategory;
  priority: TicketPriority;
  orderId?: string;
  designId?: string;
}
