// src/types/chatTypes.ts

export interface ChatMessage {
    messageId?: number;
    conversationId: number;
    senderId: number;
    content: string;
    sentAt: string | Date; // API trả về string, nhưng có thể convert sang Date
}

export interface ChatConversation {
    conversationId: number;
    name?: string;
    conversationType: string;
    lastMessage?: ChatMessage;
    userAId?: number;
    userBId?: number;
}

export interface ChatMessageDto {
    messageId?: number;
    conversationId: number;
    senderId: number;
    content: string;
    sentAt: string;
}