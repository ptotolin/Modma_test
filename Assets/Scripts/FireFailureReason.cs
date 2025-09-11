public enum FireFailureReason
{ 
    NoReason      = 0,
    NoAmmo        = 1,
    Reloading     = 1 << 1,
    OutOfRange    = 1 << 2,
    NoTarget      = 1 << 3,
    OnCooldown    = 1 << 4,
    InvalidTarget = 1 << 5,
}