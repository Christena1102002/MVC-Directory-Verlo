using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using mvc.Models.Authorize;
using Microsoft.AspNetCore.Identity;
using mvc.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System;
using mvc.ViewModels;

namespace mvc.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ProjectContext _context;

        public ChatHub(
            UserManager<ApplicationUser> userManager,
            ProjectContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<List<AdminConversationViewModel>> GetAdminConversations()
        {
            var currentUserId = Context.UserIdentifier;
            var user = await _userManager.FindByIdAsync(currentUserId);

            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
                return new List<AdminConversationViewModel>();

            return await _context.Conversations
                .Where(c => c.IsAdminBroadcast || c.AdminId == currentUserId)
                .OrderByDescending(c => c.LastMessageAt)
                .Select(c => new AdminConversationViewModel
                {
                    Id = c.Id,
                    UserName = c.User.UserName,
                    UserId = c.UserId,
                    LastMessage = c.Messages.OrderByDescending(m => m.SentAt)
                                          .Select(m => m.Content)
                                          .FirstOrDefault(),
                    LastMessageAt = c.LastMessageAt ?? c.CreatedAt,
                    UnreadCount = c.Messages.Count(m => !m.IsRead && m.SenderId != currentUserId)
                })
                .ToListAsync();
        }

        public async Task<List<UserConversationDto>> GetUserConversations()
        {
            var currentUserId = Context.UserIdentifier;
            
            if (string.IsNullOrEmpty(currentUserId))
                return new List<UserConversationDto>();

            return await _context.Conversations
                .Where(c => c.UserId == currentUserId)
                .OrderByDescending(c => c.LastMessageAt)
                .Select(c => new UserConversationDto
                {
                    Id = c.Id,
                    AdminName = c.Admin != null ? c.Admin.UserName : "Technical Support",
                    LastMessage = c.Messages
                                 .OrderByDescending(m => m.SentAt)
                                 .Select(m => m.Content)
                                 .FirstOrDefault(),
                    LastMessageAt = c.LastMessageAt ?? c.CreatedAt,
                    UnreadCount = c.Messages
                                 .Count(m => !m.IsRead &&
                                           m.SenderId != currentUserId)
                })
                .ToListAsync();
        }

        public async Task<List<ChatMessageViewModel>> GetConversationMessages(int conversationId)
        {
            var currentUserId = Context.UserIdentifier;
            
            if (string.IsNullOrEmpty(currentUserId))
                return new List<ChatMessageViewModel>();

            var isAdmin = await IsCurrentUserAdmin();
                
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.Id == conversationId);
                
            if (conversation == null || 
                (!isAdmin && conversation.UserId != currentUserId && conversation.AdminId != currentUserId))
            {
                return new List<ChatMessageViewModel>();
            }

            var unreadMessages = await _context.ChatMessages
                .Where(m => m.ConversationId == conversationId &&
                          !m.IsRead &&
                          m.SenderId != currentUserId)
                .ToListAsync();

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }
            
            if (unreadMessages.Any())
            {
                await _context.SaveChangesAsync();
            }

            return await _context.ChatMessages
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.SentAt)
                .Select(m => new ChatMessageViewModel
                {
                    Id = m.Id,
                    Content = m.Content,
                    SenderId = m.SenderId,
                    SentAt = m.SentAt,
                    IsRead = m.IsRead
                })
                .ToListAsync();
        }

        public async Task SendMessageToConversation(int conversationId, string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            
            var currentUserId = Context.UserIdentifier;
            
            if (string.IsNullOrEmpty(currentUserId)) return;
            
            var conversation = await _context.Conversations
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == conversationId && c.UserId == currentUserId);
                
            if (conversation == null) return;
            
            var chatMessage = new ChatMessage
            {
                ConversationId = conversationId,
                SenderId = currentUserId,
                Content = message.Trim(),
                SentAt = DateTime.UtcNow,
                IsRead = false
            };
            
            _context.ChatMessages.Add(chatMessage);
            conversation.LastMessageAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            await Clients.Group("Admins").SendAsync("ReceiveAdminMessage", new
            {
                ConversationId = conversationId,
                Message = message,
                SenderId = currentUserId,
                SenderName = conversation.User.UserName,
                UserId = currentUserId,
                Timestamp = DateTime.UtcNow
            });
        }

        public async Task SendToAdmins(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            
            var currentUserId = Context.UserIdentifier;
            
            if (string.IsNullOrEmpty(currentUserId)) return;
            
            var user = await _userManager.FindByIdAsync(currentUserId);
            
            if (user == null) return;

            var conversation = new Conversation
            {
                UserId = currentUserId,
                IsAdminBroadcast = true,
                CreatedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            var chatMessage = new ChatMessage
            {
                ConversationId = conversation.Id,
                SenderId = currentUserId,
                Content = message.Trim(),
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            await Clients.Group("Admins").SendAsync("ReceiveAdminMessage", new
            {
                ConversationId = conversation.Id,
                Message = message,
                SenderId = currentUserId,
                SenderName = user.UserName,
                UserId = currentUserId,
                Timestamp = DateTime.UtcNow
            });
        }

        public async Task SendReply(int conversationId, string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            
            var currentUserId = Context.UserIdentifier;
            
            if (string.IsNullOrEmpty(currentUserId)) return;
            
            if (!await IsCurrentUserAdmin()) return;

            var conversation = await _context.Conversations
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null) return;

            if (string.IsNullOrEmpty(conversation.AdminId))
            {
                conversation.AdminId = currentUserId;
            }

            var chatMessage = new ChatMessage
            {
                ConversationId = conversationId,
                SenderId = currentUserId,
                Content = message.Trim(),
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.ChatMessages.Add(chatMessage);
            conversation.LastMessageAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var adminUser = await _userManager.FindByIdAsync(currentUserId);
            await Clients.User(conversation.UserId).SendAsync("ReceiveReply", new
            {
                Message = message,
                AdminName = adminUser?.UserName ?? "Technical Support",
                Timestamp = DateTime.UtcNow,
                ConversationId = conversationId
            });

            await Clients.Group("Admins").SendAsync("UpdateAdminConversation", new
            {
                ConversationId = conversationId,
                UserId = conversation.UserId,
                UserName = conversation.User.UserName,
                LastMessage = message,
                LastMessageAt = DateTime.UtcNow,
                AdminName = adminUser?.UserName ?? "Technical Support"
            });
        }
        
        public override async Task OnConnectedAsync()
        {
            try
            {
                var user = await _userManager.GetUserAsync(Context.User);
                if (user != null)
                {
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
                        await Clients.Group("Admins").SendAsync("AdminConnected", user.UserName);
                    }
                    else
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, "Users");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnConnectedAsync: {ex.Message}");
            }
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                var user = await _userManager.GetUserAsync(Context.User);
                if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    await Clients.Group("Admins").SendAsync("AdminDisconnected", user.UserName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnDisconnectedAsync: {ex.Message}");
            }
            
            await base.OnDisconnectedAsync(exception);
        }
        
        private async Task<bool> IsCurrentUserAdmin()
        {
            var user = await _userManager.GetUserAsync(Context.User);
            return user != null && await _userManager.IsInRoleAsync(user, "Admin");
        }

        public async Task SendMessage(int conversationId, string message)
        {
            await Clients.Group($"conversation_{conversationId}").SendAsync("ReceiveMessage", new
            {
                ConversationId = conversationId,
                Content = message,
                Timestamp = DateTime.UtcNow,
                SenderId = Context.UserIdentifier,
                IsOutgoing = false 
            });
        }

        public async Task JoinConversation(int conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
        }

        public async Task LeaveConversation(int conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
        }

        [Authorize(Roles = "Admin")]
        public async Task JoinAdminChannel()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admin_channel");
        }

        public async Task UpdateConversationStatus(int conversationId, string status)
        {
            await Clients.Group("admin_channel").SendAsync("ConversationUpdated", conversationId);
        }
    }

    public class UserConversationDto
    {
        public int Id { get; set; }
        public string AdminName { get; set; }
        public string LastMessage { get; set; }
        public DateTime LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
    }
}