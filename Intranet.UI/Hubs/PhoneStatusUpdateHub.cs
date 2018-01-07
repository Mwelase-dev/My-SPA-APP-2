using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
//using Owin;

namespace Intranet.UI.Hubs
{

    public class PhoneHub : Hub
    {
        public void Send(string name, string message, Guid staffId)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<PhoneHub>();
            hubContext.Clients.All.addMessage(name, message, staffId);
        }

        //public void PrivateSend(string userId, string message, Guid staffId)
        //{
        //    userId = Context.ConnectionId;
        //    var hubContext = GlobalHost.ConnectionManager.GetHubContext<PhoneHub>();
        //    hubContext.Clients.User(userId).sendPrivateMessage(userId, message, staffId);
        //}


        public void BroadcastToGroup(string message, string group)
        {
            Clients.Group(group).newMessageReceived(message);
        }

        public Task JoinGroup(string groupName)
        {
            return Groups.Add(Context.ConnectionId, groupName);
        }

        public Task LeaveGroup(string groupName)
        {
            return Groups.Remove(Context.ConnectionId, groupName);
        }

        public override Task OnConnected()
        {
            return Clients.All.joined(GetAuthInfo());
        }

        //public override Task OnDisconnected(bool stopCalled)
        //{
        //    return Clients.All.joined(GetAuthInfo());
        //}

        protected object GetAuthInfo()
        {
            var user = Context.User;
            return new
            {
                IsAuthenticated = user.Identity.IsAuthenticated,
                IsAdmin = user.IsInRole("Admin"),
                UserName = user.Identity.Name

            };
        }
    }
}