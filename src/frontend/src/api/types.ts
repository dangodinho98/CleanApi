export type ItemResponse = {
  id: string;
  title: string;
  description: string;
  createdAtUtc: string;
  ownerUserId: string | null;
};

export type AuthResponse = {
  token: string;
  user: {
    id: string;
    email: string;
    displayName: string;
  };
};

export type UserSummary = {
  id: string;
  email: string;
  displayName: string;
};
