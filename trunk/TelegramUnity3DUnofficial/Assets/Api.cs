using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Api
{/*
	public class Auth
	{
		/// <summary>
		/// Returns the system registration status of the phone number passed to it.
		/// </summary>
		/// <returns>The method returns an auth.CheckedPhone type object with information on whether an account with such a phone number has already been registered, as well as whether invitations were sent to this number (using the auth.sendInvites method).</returns>
		/// <param name="PhoneNumber">Phone number in the international format</param>
		public SharpTelegram.Schema.AuthCheckedPhone CheckPhone(string PhoneNumber)
		{
			return null;
		}

		/// <summary>
		/// Sends an confirmation code message to the specified phone number via SMS.
		/// </summary>
		/// <param name="phoneNumber">Phone number in international format.</param>
		/// <param name="smsType">Message text type.	Possible values:	0 - message contains a numerical code, 1 (deprecated) - message contains a link {app_name}://{code}, 5 - message sent via Telegram instead of SMS (the (auth.sentAppCode) constructor may be returned in this case)</param>
		/// <param name="apiId">Application identifier.</param>
		/// <param name="apiHash">Application secret hash.</param>
		/// <param name="langCode">Code for the language used on a client, ISO 639-1 standard Parameter added in layer 5.</param>
		public SharpTelegram.Schema.AuthSentCode SendCode(string phoneNumber, int smsType, int apiId, string apiHash, string langCode)
		{
			return null;
		}

		/// <summary>
		/// Forces sending an SMS message to the specified phone number. Use this method if auth.sentAppCode was returned as a response to auth.sendCode, but the user can't reach the device with Telegram.
		/// </summary>
		/// <returns>Sending successful.</returns>
		/// <param name="phoneNumber">Phone number in international format.</param>
		/// <param name="phoneCodeHash">SMS-message ID.</param>
		public bool SendSms(string phoneNumber, string phoneCodeHash)
		{
			return false;
		}

		/// <summary>
		/// Makes a voice call to the passed phone number. A robot will repeat the confirmation code from a previously sent SMS message.
		/// </summary>
		/// <returns>The call.</returns>
		/// <param name="phoneNumber">Phone number in the international format.</param>
		/// <param name="phoneCodeHash">SMS-message ID.</param>
		public bool SendCall(string phoneNumber, string phoneCodeHash)
		{
			return false;
		}

		/// <summary>
		/// Registers a validated phone number in the system.
		/// </summary>
		/// <returns>Returns an auth.Authorization object with information about the new authorization.</returns>
		public SharpTelegram.Schema.AuthAuthorization SignUp(string phoneNumber, string phoneCodeHash, string phoneCode, string firstName, string lastName)
		{
			return null;
		}

		/// <summary>
		/// Signs in a user with a validated phone number.
		/// </summary>
		/// <returns>The in.</returns>
		/// <param name="phoneNumber">Phone number in the international format.</param>
		/// <param name="phoneCodeHash">SMS-message ID..</param>
		/// <param name="phoneCode">Valid numerical code from the SMS-message.</param>
		public SharpTelegram.Schema.AuthAuthorization SignIn(string phoneNumber, string phoneCodeHash, string phoneCode)
		{
			return null;
		}

		/// <summary>
		/// Logs out the user.
		/// </summary>
		/// <returns>Logout success.</returns>
		public bool LogOut()
		{
			return false;
		}

		/// <summary>
		/// Terminates all user's authorized sessions except for the current one. After calling this method it is necessary to reregister the current device using the method account.registerDevice
		/// </summary>
		/// <returns>Reset successful.</returns>
		public bool ResetAuthorizations()
		{
			return false;
		}

		/// <summary>
		/// Binds a temporary authorization key temp_auth_key_id to the permanent authorization key perm_auth_key_id.Each permanent key may only be bound to one temporary key at a time, binding a new temporary key overwrites the previous one.
		/// </summary>
		/// <returns>The temp auth key.</returns>
		/// <param name="permAuthKeyId">Permanent auth_key_id to bind to.</param>
		/// <param name="nonce">Random long from Binding message contents.</param>
		/// <param name="expiresAt">Unix timestamp to invalidate temporary key, see Binding message contents.</param>
		/// <param name="encryptedMessage">See Generating encrypted_message: https://core.telegram.org/method/auth.bindTempAuthKey</param>
		public bool BindTempAuthKey(long permAuthKeyId, long nonce, int expiresAt, byte[] encryptedMessage)
		{
			return false;
		}
	}
	public class Account
	{
		public class FUserFull
		{
			public FUser User;

			public bool Blocked = false;
			public string RealFirstName = "";
			public string RealLastName = "";
		}

		public class FUser
		{
			public class FUserStatus
			{
				public int userStatusOnline = 0; // Time to expire
				public bool IsOnline
				{
					get
					{
						return userStatusOnline > 0;
					}
				}
			}

			public class FUserProfilePhoto
			{
				public class FFileLocation
				{
					public int dcId = 0;
					public long VolumeId;
					public int LocalId;
					public long Secret;
				}
				public long photoId;
				public FFileLocation PhotoSmall;
				public FFileLocation PhotoBig;
			}
			public int Id = 0;
			public string FirstName = "";
			public string LastName = "";
			public long _AccessHash = 0; // Checksum, dependant on user ID
			public string Phone = "";
			public FUserProfilePhoto Photo;
			public FUserStatus Status;
			public string Username = "";
		}

		public class FUserProfile
		{
		
		}

		public class FInputNotifyPeer
		{

		}

		public class FInputPeerNotifySettings
		{
		}

		public enum FTokenType : int
		{
			APNS = 1,
			GCM = 2,
			MPNS = 3,
			SimplePush = 4,
			UbuntuPhone = 5,
			Blackberry = 6
		}

		/// <summary>
		/// Registers a device to send it PUSH-notifications in the future.
		/// </summary>
		/// <returns>Registration successful.</returns>
		public bool RegisterDevice(FTokenType tokenType, string token, string deviceModel, string systemVersion, string appVersion, bool appSandbox, string langCode)
		{
			return false;
		}

		/// <summary>
		/// Deletes a device by its token, stops sending PUSH-notifications to it.
		/// </summary>
		/// <returns><c>true</c>, if device was unregistered, <c>false</c> otherwise.</returns>
		/// <param name="pTokenType">Device token type.</param>
		/// <param name="token">Device token.</param>
		public bool UnregisterDevice(FTokenType tokenType, string token)
		{
			return false;
		}

		/// <summary>
		/// Edits notification settings from a given user/group, from all users/all groups.
		/// </summary>
		/// <returns><c>true</c>, if notify settings was updated, <c>false</c> otherwise.</returns>
		public bool UpdateNotifySettings(FInputNotifyPeer InputNotifyPeer, FInputPeerNotifySettings InputPeerNotifySettings)
		{
			return false;
		}

		/// <summary>
		/// Resets all notification settings from users and groups.
		/// </summary>
		/// <returns><c>true</c>, if notify settings was reset, <c>false</c> otherwise.</returns>
		public bool ResetNotifySettings()
		{
			return false;
		}

		/// <summary>
		/// Updates user profile.
		/// </summary>
		/// <returns>Returns User object containing the updated current user profile.</returns>
		/// <param name="NewFirstName">New first name.</param>
		/// <param name="NewLastName">New last name.</param>
		public SharpTelegram.Schema.IUser UpdateProfile(string NewFirstName, string NewLastName)
		{
			return null;
		}

		/// <summary>
		/// Updates online user status.
		/// </summary>
		/// <returns><c>true</c>, if status was updated, <c>false</c> otherwise.</returns>
		/// <param name="UserOffline">If set to <c>true</c> user offline.</param>
		public bool UpdateStatus(bool UserOffline)
		{
			return false;
		}
	}
	
	public class Users
	{
		/// <summary>
		/// Returns basic user info according to their identifiers.
		/// </summary>
		/// <returns>List of users filtered by id.</returns>
		/// <param name="Ids">Identifiers.</param>
		public List<long> GetUsers(List<long> Ids)
		{
			return null;
		}

		/// <summary>
		/// Returns extended user info by ID.
		/// </summary>
		/// <returns>Returns a UserFull object containing user info..</returns>
		public SharpTelegram.Schema.UserFull GetFullUser()
		{
			return null;
		}
	}

	public class Contacts
	{
		/// <summary>
		/// Returns the list of contact statuses.
		/// </summary>
		/// <returns>List<FContactStatus>.</returns>
		public List<SharpTelegram.Schema.ContactStatus> GetStatuses()
		{
			return null;
		}

		/// <summary>
		/// Returns the current user's contact list.
		/// </summary>
		public void GetContacts()
		{

		}

		/// <summary>
		/// Deletes a contact from the list.
		/// </summary>
		/// <param name="UserId">User identifier.</param>
		public void DeleteContact(int UserId)
		{
			return false;
		}

		/// <summary>
		/// Adds the user to the blacklist.
		/// </summary>
		/// <param name="UserId">User identifier.</param>
		public bool Block(int UserId)
		{
			return false;
		}

		/// <summary>
		/// Deletes the user from the blacklist.
		/// </summary>
		/// <returns>Unblock successful.</returns>
		/// <param name="UserId">User identifier.</param>
		public bool UnBlock(int UserId)
		{
			return false;
		}

		/// <summary>
		/// Returns the list of blocked users.
		/// </summary>
		/// <returns>Blocked user list.</returns>
		/// <param name="Offset">Offset.</param>
		/// <param name="Limit">Limit.</param>
		public SharpTelegram.Schema.ContactBlocked GetBlocked(int Offset, int Limit)
		{
			return null;
		}

		public SharpTelegram.Schema.IContactsImportedContacts ImportContacts(List<SharpTelegram.Schema.ImportedContact> Contacts, bool Replace)
		{

		}
	}

	public class Messages
	{
		private long SendMessageCount = 0;

		/// <summary>
		/// Sends a text message.
		/// </summary>
		/// <param name="UserId">User identifier.</param>
		/// <param name="Message">Text message.</param>
		public bool SendMessage(int UserId, string Message) // long RandomId) -> use internal SendMessageCount
		{
			SendMessageCount++;
			return false;
		}

		/// <summary>
		/// Sends a current user typing event (see SendMessageAction for all event types) to a conversation partner or group.
		/// </summary>
		/// <returns><c>true</c>, if typing was set, <c>false</c> otherwise.</returns>
		/// <param name="TargetUserId">Target user identifier.</param>
		/// <param name="Typing">D.</param>
		public bool SetTyping(int TargetUserId) // action	SendMessageAction, sendMessageTypingAction#16bf744e = SendMessageAction;
		{
			return false;
		}

		/// <summary>
		/// Returns the list of messages by their IDs.
		/// </summary>
		/// <returns>The messages.</returns>
		/// <param name="MessageIdList">Messages ids list.</param>
		public SharpTelegram.Schema.MessagesMessages GetMessages(List<int> MessageIdList)
		{
			return null;
		}

		/// <summary>
		/// Returns message history for a chat.
		/// </summary>
		/// <returns>Returns chat history.</returns>
		/// <param name="UserId">Target user or group.</param>
		/// <param name="Offset">Number of list elements to be skipped. As of Layer 15 this value is added to the one that was calculated from max_id. Negative values are also accepted..</param>
		/// <param name="MaxId">If a positive value was transferred, the method will return only messages with IDs less than max_id.</param>
		/// <param name="Limit">Number of list elements to be returned.</param>
		public SharpTelegram.Schema.MessagesMessages GetHistory(int UserId, int Offset, int MaxId, int Limit)
		{
			return null;
		}

		/// <summary>
		/// Returns search messages.
		/// </summary>
		/// <param name="UserId">User or chat, histories with which are searched, or (inputPeerEmpty) constructor for global search.</param>
		/// <param name="Query">Text search request.</param>
		/// <param name="Filter">Additional filter.</param>
		/// <param name="MinDate">If a positive value was transferred, only messages with a sending date bigger than the transferred one will be returned.</param>
		/// <param name="MaxDate">If a positive value was transferred, only messages with a sending date less than the transferred one will be returned.</param>
		/// <param name="Offset">Number of list elements to be skipped.</param>
		/// <param name="MaxId">If a positive value was transferred, the method will return only messages with IDs less than the set ones.</param>
		/// <param name="Limit">Number of list elements to be returned.</param>
		public SharpTelegram.Schema.MessagesMessages Search(int UserId, string Query, int Filter, int MinDate, int MaxDate, int Offset, int MaxId, int Limit)
		{
			return null;
		}

		/// <summary>
		/// Marks message history as read.
		/// </summary>
		/// <param name="UserId">Target user or group.</param>
		/// <param name="MaxId">If a positive value is passed, only messages with identifiers less or equal than the given one will be read.</param>
		/// <param name="Offset">Offset.</param>
		/// <param name="ReadContents">If set to <c>true</c> read contents.</param>
		public SharpTelegram.Schema.MessagesAffectedHistory ReadHistory(int UserId, int MaxId, int Offset, bool ReadContents)
		{
			return null;
		}

		/// <summary>
		/// Notifies the sender about the recipient having listened a voice message or watched a video.
		/// </summary>
		/// <returns>The method returns the list of successfully deleted messages in List<int>.</returns>
		/// <param name="MessageIdsToCheck">Deletes messages by their identifiers.</param>
		public List<int> ReadMessageContents(List<int> MessageIdsToCheck)
		{
			return null;
		}

		/// <summary>
		/// Deletes messages by their identifiers.
		/// </summary>
		/// <returns>The messages.</returns>
		/// <param name="MessageIdsToDelete">Message identifiers to delete.</param>
		public List<int> DeleteMessages(List<int> MessageIdsToDelete)
		{
			return null;
		}

		/// <summary>
		/// Confirms receipt of messages by a client, cancels PUSH-notification sending.
		/// </summary>
		/// <param name="MaxId">Maximum message ID available in a client.</param>
		public List<int> ReceivedMessages(int MaxId)
		{
			return null;
		}
	}

	public class Updates
	{
		public SharpTelegram.Schema.UpdatesState GetState()
		{
			return null;
		}

		public SharpTelegram.Schema.UpdatesDifference GetDifference(int Pts, int Date, int Qts)
		{
			return null;
		}
	}*/
}
