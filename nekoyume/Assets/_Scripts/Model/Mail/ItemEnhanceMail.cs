using System;
using Nekoyume.Action;

namespace Nekoyume.Model.Mail
{
    [Serializable]
    public class ItemEnhanceMail : AttachmentMail
    {
        protected override string TypeId => "itemEnhance";
        public override MailType MailType => MailType.Workshop;

        public ItemEnhanceMail(AttachmentActionResult attachmentActionResult, long blockIndex) : base(attachmentActionResult, blockIndex)
        {

        }

        public ItemEnhanceMail(Bencodex.Types.Dictionary serialized)
            : base(serialized)
        {
        }

        public override void Read(IMail mail)
        {
            mail.Read(this);
        }
    }
}
