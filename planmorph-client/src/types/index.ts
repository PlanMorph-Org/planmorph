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

// ── Student / Mentorship Types ──

export type StudentType = 'Architecture' | 'Engineering';
export type EnrollmentStatus = 'Enrolled' | 'Graduated' | 'Intern';
export type ApplicationStatus = 'Pending' | 'UnderReview' | 'Approved' | 'Rejected';
export type MentorshipStatus = 'Unmatched' | 'Matched' | 'Active' | 'Suspended';

export type ProjectStatus =
  | 'Draft' | 'Submitted' | 'UnderReview' | 'Scoped' | 'Published'
  | 'Claimed' | 'StudentAssigned' | 'InProgress'
  | 'UnderMentorReview' | 'RevisionRequested' | 'MentorApproved'
  | 'ClientReview' | 'ClientRevisionRequested'
  | 'Completed' | 'Paid' | 'Disputed' | 'Cancelled';

export type PaymentStatus =
  | 'Pending' | 'Escrowed' | 'MentorReleased' | 'StudentReleased'
  | 'Completed' | 'Disputed' | 'Refunded';

export type ProjectType = 'CustomCommission' | 'DesignModification';
export type DesignCategory = 'Residential' | 'Commercial' | 'Industrial' | 'Institutional' | 'Mixed';
export type ProjectPriority = 'Low' | 'Medium' | 'High';
export type IterationStatus = 'Submitted' | 'UnderReview' | 'Approved' | 'RevisionRequested' | 'Superseded';

export interface StudentProfile {
  id: string;
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  studentType: StudentType;
  universityName: string;
  enrollmentStatus: EnrollmentStatus;
  expectedGraduation?: string;
  studentIdNumber?: string;
  mentorId?: string;
  mentorName?: string;
  mentorshipStatus: MentorshipStatus;
  totalProjectsCompleted: number;
  averageRating: number;
  totalEarnings: number;
  createdAt: string;
}

export interface StudentApplication {
  id: string;
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  studentType: StudentType;
  universityName: string;
  studentIdNumber?: string;
  portfolioUrl?: string;
  status: ApplicationStatus;
  reviewNotes?: string;
  reviewedAt?: string;
  createdAt: string;
}

export interface MentorshipProject {
  id: string;
  projectNumber: string;
  title: string;
  description: string;
  requirements?: string;
  projectType: ProjectType;
  category: DesignCategory;
  status: ProjectStatus;
  scope?: string;
  estimatedDeliveryDays: number;
  clientFee: number;
  mentorFee: number;
  studentFee: number;
  priority: ProjectPriority;
  maxRevisions: number;
  currentRevisionCount: number;
  clientName?: string;
  mentorName?: string;
  studentId?: string;
  studentName?: string;
  mentorDeadline?: string;
  studentDeadline?: string;
  completedAt?: string;
  paymentStatus: PaymentStatus;
  createdAt: string;
}

export interface ProjectIteration {
  id: string;
  projectId: string;
  iterationNumber: number;
  submittedById: string;
  submittedByRole: string;
  status: IterationStatus;
  notes?: string;
  reviewNotes?: string;
  reviewedById?: string;
  reviewedAt?: string;
  createdAt: string;
}

export interface ProjectMessage {
  id: string;
  projectId: string;
  senderId: string;
  senderName: string;
  senderRole: string;
  content: string;
  isSystemMessage: boolean;
  createdAt: string;
}
