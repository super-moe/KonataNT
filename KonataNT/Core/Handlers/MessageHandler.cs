using KonataNT.Core.Packet.Message;
using KonataNT.Events;
using KonataNT.Events.EventArgs;
using KonataNT.Message;
using KonataNT.Message.Chains;

namespace KonataNT.Core.Handlers;

internal class MessageHandler(BaseClient client)
{
    private const string Tag = nameof(MessageHandler);
    
    public EventBase Parse(PushMsgBody msg)
    {
        string name = msg.ResponseHead.Grp?.MemberCard ?? "";
        uint timestamp = msg.ContentHead.Timestamp;
        var @struct = new MessageStruct(msg.ResponseHead.FromUin, name, DateTime.Now)
        {
            Time = DateTime.UnixEpoch.AddSeconds(timestamp),
            Sequence = msg.ContentHead.Sequence,
            SourceType = msg.ResponseHead.Grp is not null ? MessageStruct.Source.Group : MessageStruct.Source.Friend
        };

        if (msg.Body.RichText?.Elems is { } elems)
        {
            foreach (var elem in elems) switch (elem)
            {
                case { Text: { Attr6Buf: { } extra } text }:  // at
                {
                    break;
                }
                case { Text: { } text }:  // raw text
                {
                    if (@struct.Chain.FirstOrDefault(x => x is TextChain) is { } existing)
                    {
                        ((TextChain)existing).Combine(TextChain.Create(text.Str ?? ""));
                    }
                    else
                    {
                        @struct.Chain.Add(TextChain.Create(text.Str ?? ""));
                    }
                    break;
                }
                case { NotOnlineImage: { } image }:  // private image
                {
                    break;
                }
                case { CustomFace: { } image }:  // group image
                {
                    break;
                }
                case { CommonElem: { } common }:  //  tx's new type shit
                {
                    switch (common.BusinessType)
                    {
                        
                    }
                        
                    break;
                }
            }
        }
        
        client.Logger.LogInformation(Tag, $"Received message from {name} ({msg.ResponseHead.FromUin}): {@struct.Chain.ToPreviewString()}");

        return @struct.SourceType switch
        {
            MessageStruct.Source.Group => new BotGroupMessageEvent(msg.ResponseHead.Grp?.GroupUin ?? 0, msg.ResponseHead.FromUin, @struct),
            MessageStruct.Source.Friend => new BotPrivateMessageEvent(msg.ResponseHead.FromUin, @struct),
            MessageStruct.Source.Stranger => throw new NotImplementedException("干什么！"),
            _ => throw new NotImplementedException()
        };
    }

    public MessageChain Build(MessageStruct @struct)
    {
        throw new NotImplementedException();
    }
}