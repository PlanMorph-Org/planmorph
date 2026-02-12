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
