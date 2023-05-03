using HotChocolate.Types;

namespace Adform.Bloom.Api.Graph.User
{
    public class UserType : ObjectType<Contracts.Output.User>
    {
        protected override void Configure(IObjectTypeDescriptor<Contracts.Output.User> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Name(nameof(Contracts.Output.User));

            descriptor.Field(t => t.Id)
                .Type<NonNullType<IdType>>().Description("Id of the User.");

            descriptor.Field(t => t.Username)
                .Type<NonNullType<StringType>>().Description("User Username.");

            descriptor.Field(t => t.Name)
                .Type<NonNullType<StringType>>().Description("User Name.");

            descriptor.Field(t => t.FirstName)
                .Type<NonNullType<StringType>>().Description("User First Name.");

            descriptor.Field(t => t.LastName)
                .Type<NonNullType<StringType>>().Description("User Last Name.");

            descriptor.Field(t => t.Email)
                .Type<NonNullType<StringType>>().Description("User Email.");

            descriptor.Field(t => t.Enabled)
                .Type<NonNullType<BooleanType>>().Description("User Enabled.");

            descriptor.Field(t => t.Phone)
                .Type<StringType>().Description("User Phone.");

            descriptor.Field(t => t.Timezone)
                .Type<NonNullType<StringType>>().Description("User Timezone.");

            descriptor.Field(t => t.Locale)
                .Type<NonNullType<StringType>>().Description("User Locale.");

            descriptor.Field(t => t.Company)
                .Type<NonNullType<StringType>>().Description("User Company.");

            descriptor.Field(t => t.TwoFaEnabled)
                .Type<NonNullType<BooleanType>>().Description("User has 2 FA enabled.");

            descriptor.Field(t => t.SecurityNotificationsEnabled)
                .Type<NonNullType<BooleanType>>().Description("User has Security Notifications enabled.");

            descriptor.Field(t => t.Status)
                .Type<NonNullType<UserStatusTypeEnum>>().Description("User Status.");
            
            descriptor.Field(t => t.Type)
                .Type<NonNullType<UserTypeTypeEnum>>().Description("User Type.");

            descriptor.Field(t => t.TncReadAndAccepted)
                .Type<NonNullType<BooleanType>>().Description("User has read/accepted the \"Terms & Conditions\"");

            descriptor.Field(t => t.CreatedAt).Ignore();

            descriptor.Field(t => t.UpdatedAt).Ignore();
        }
    }
}