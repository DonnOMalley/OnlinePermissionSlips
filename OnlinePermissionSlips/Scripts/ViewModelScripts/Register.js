function UpdateUserName(EmailAddress, UserNameElement) {
	if (UserNameElement != null && UserNameElement.value === "") {
		UserNameElement.value = EmailAddress;
	}
}